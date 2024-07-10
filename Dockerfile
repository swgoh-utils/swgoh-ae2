# best practices from https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-6.0#the-dockerfile

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY swgoh-ae-api.sln ./swgoh-ae-api.sln
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

FROM mcr.microsoft.com/dotnet/sdk:8.0
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
