# SWGoH Asset Extractor
This Program can download and extract SWGoH 2D Textures.

Version for RaspberryPi can be found at https://github.com/Lego-Fan9/swgoh-assetapi
## Docker

Docker images are automatically built and publish to github's container registry.  You can pull down an image and get started immediately using something like this:

```
docker pull ghcr.io/swgoh-utils/swgoh-ae2:latest
docker run --name=swgoh-ae2 \
  -d \
  -restart always \
  -p 3000:8080 \
  ghcr.io/swgoh-utils/swgoh-ae2:latest
```

### Open API / Swagger file

The swagger document is served at `/swagger` from the container

### Endpoints
Downloading a single asset example:
```
/Asset/single?forceReDownload=BOOL&version=ASSET_VERSION&assetName=ASSET_NAME
```

## Usage:
**SwgohAssetGetterConsole.exe -downloadManifest**
this command will download the Manifest. Make sure to fire this command when you have changed the Version

**SwgohAssetGetterConsole.exe -getAssetNames**
this command will create a file "AssetNames.json" wich includes all AssetNames that can possibly be downloaded

**SwgohAssetGetterConsole.exe -getAssetPrefixes**
this command will create a file "AssetPrefixes.json" wich includes all prefixes (to order the assets)

**SwgohAssetGetterConsole.exe -single charui_b1**
Downloads and extracts the specified AssetBundle

**SwgohAssetGetterConsole.exe -downloadDiff 2400 -v 2402**
Downloads and extracts all Assets that are new in Version 2402 compared to version 2400

**SwgohAssetGetterConsole.exe -downloadDiff 2400 all -v 2402**
Downloads and extracts all Assets that are different(new or changed) in Version 2402 compared to version 2400

**SwgohAssetGetterConsole.exe -downloadDiff 2400 changed -v 2402**
Downloads and extracts all Assets that are changed in Version 2402 compared to version 2400

**SwgohAssetGetterConsole.exe -downloadDiff 2400 new -v 2402**
Downloads and extracts all Assets that are new in Version 2402 compared to version 2400

**SwgohAssetGetterConsole.exe -prefix charui**
Downloads and extracts all Assets from the current Manifest with a specific prefix(here charui)

**SwgohAssetGetterConsole.exe -all**
Downloads and extracts all Assets from the current Manifest

### Optional Parameters
**-version**
**-v**
Sets the target assetversion for this run only it overrides the default from settings.json

**-target**
**-t**
Sets the target Targetfolder(outputfolder) for this run only it overrides the default from settings.json

**-workingfolder**
**-w**
Sets the target Workingfolder for this run only it overrides the default from settings.json

**-ExportMeshes**
**-eM**
Sets the parameter that defines if Meshes are downloaded or not. For this run only it overrides the default from settings.json

### .NetCore usage:
similar to windows usage. Just call the dll with dotnet instead of the exe:
**dotnet SwgohAssetGetterConsole.dll -downloadManifest**
since this is build with .NetCore you will need the .Net Core runtime

https://dotnet.microsoft.com/download

### linux dependencies:
sudo apt install libgdiplus

### macos dependencies:
brew install mono-libgdiplus
brew install libmagic

### other usage:

if you want to use the Program on a targetsystem that doesnt have dotnet runtime installed you need to rebuild with a specific target e.g.:
**dotnet build --runtime ubuntu.16.04-x64**

Before first usage please make sure to sett the correct settings in settings.json
