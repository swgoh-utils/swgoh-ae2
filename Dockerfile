FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["./", "./"]
WORKDIR /src/AssetWebApi
RUN dotnet restore "AssetWebApi.csproj"
RUN dotnet build "AssetWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AssetWebApi.csproj" -c Release -o /app/publish

FROM base AS final
RUN apt update && apt install -y \
        libc6-dev \
        libgdiplus \
        libx11-dev \
        && rm -rf /var/lib/apt/lists/*
WORKDIR /app

COPY --from=publish /app/publish .
COPY --from=publish /app/publish/x64 /usr/share/dotnet/x64
COPY --from=publish /app/publish/x86 /usr/share/dotnet/x86

ENTRYPOINT ["dotnet", "AssetWebApi.dll"]
