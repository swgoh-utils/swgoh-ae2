# best practices from https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-6.0#the-dockerfile

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY SwgohAssetGetterConsole/*.csproj ./SwgohAssetGetterConsole/
COPY AssetGetterTools/*.csproj ./AssetGetterTools/
COPY AssetWebApi/*.csproj ./AssetWebApi/
COPY AssetStudio/AssetStudio/*.csproj ./AssetStudio/AssetStudio/
COPY AssetStudio/AssetStudioFBXWrapper/*.csproj ./AssetStudio/AssetStudioFBXWrapper/
COPY AssetStudio/AssetStudioUtility/*.csproj ./AssetStudio/AssetStudioUtility/
COPY AssetStudio/AssetStudio.PInvoke/*.csproj ./AssetStudio/AssetStudio.PInvoke/
COPY AssetStudio/Texture2DDecoderWrapper/*.csproj ./AssetStudio/Texture2DDecoderWrapper/
RUN dotnet restore

# copy everything else and build app
COPY ["./", "./"]
WORKDIR /source/AssetWebApi
RUN dotnet publish "AssetWebApi.csproj" -c release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim
WORKDIR /app
EXPOSE 80
ENV DISABLE_HTTPS_REDIRECT="true"
ENV ENABLE_SWAGGER="true"
RUN apt update && apt install -y \
  libc6-dev \
  libgdiplus \
  libx11-dev \
  && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /app/publish/x64 /usr/share/dotnet/x64
COPY --from=build /app/publish/x86 /usr/share/dotnet/x86

ENTRYPOINT ["dotnet", "AssetWebApi.dll"]