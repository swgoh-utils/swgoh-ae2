using Asset_Getter;
using AssetGetterTools;
using AssetGetterTools.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SwgohAssetGetterConsole
{
    public class MainProgram
    {
        public string workingFolder { get; set; }
        public string targetFolder { get; set; }
        public string AssetVersion { get; set; }

        public bool exportMeshes { get; set; }
        public bool exportSpriteAtlases { get; set; }
        public AssetOS assetOS { get; set; }

        public string AssetDownloadUrl
        {
            get
            {
                return @"https://eaassets-a.akamaihd.net/assetssw.capitalgames.com/PROD/" + this.AssetVersion + this.AssetOSPath;
            }
        }
        public string AssetOSPath { get; set; }

        public Filehelper fileHelper { get; set; }

        public MainProgram()
        {
            var defaultSettings = DefaultSettings.GetDefaultSettings();

            this.workingFolder = defaultSettings.workingDirectory;
            this.targetFolder = defaultSettings.defaultOutputDirectory;
            this.AssetVersion = defaultSettings.defaultAssetVersion;
            this.exportMeshes = defaultSettings.exportMeshes;
            this.assetOS = defaultSettings.assetOS;

            this.fileHelper = new Filehelper();
            this.fileHelper.workingFolder = defaultSettings.workingDirectory;
            this.SetAssetOSPath(assetOS);
        }

        public void SetAssetOSPath(AssetOS assetOS)
        {
            switch (assetOS)
            {
                case AssetOS.Windows:
                    AssetOSPath = @"/Windows/ETC/";
                    break;
                case AssetOS.Android:
                    AssetOSPath = @"/Android/ETC/";
                    break;
                case AssetOS.iOS:
                    AssetOSPath = @"/iOS/PVRTC/";
                    break;
                default:
                    AssetOSPath = @"/Windows/ETC/";
                    break;
            }
        }

        public void SaveAssetNamesToFile()
        {
            var allAssets = this.GetAssetsFromManifest();
            File.WriteAllText($"{workingFolder}AssetNames.json", JsonConvert.SerializeObject(allAssets));
        }

        public void SavePrefixesToFile()
        {
            var allPrefixes = this.GetPrefixesFromManifest();
            File.WriteAllText($"{workingFolder}AssetPrefixes.json", JsonConvert.SerializeObject(allPrefixes));
        }

        public void exportAllFiles()
        {
            var allAssets = GetAssetsFromManifest();
            foreach (var asset in allAssets)
            {
                exportSingleFile(asset);
            }
        }

        public void downloadAllAudioFiles()
        {
            var allAssets = GetAudioFromManifest();
            foreach (var asset in allAssets)
            {
                DownloadAudioPackages(asset);
            }
        }

        public void exportAllWithPrefixFile(string prefix)
        {
            var allAssetsWithPrefix = GetAssetsFromManifest().Where(r => r.StartsWith(prefix)).ToList();
            foreach (var asset in allAssetsWithPrefix)
            {
                exportSingleFile(asset);
            }
        }

        public void exportMultipleAssets(List<string> allAssetsToExport)
        {
            foreach (var asset in allAssetsToExport)
            {
                exportSingleFile(asset);
            }
        }

        public List<string> diffAssetVersions(string oldmanifest, string newManifest)
        {
            ManifestHelper manifestHelper = new ManifestHelper();
            manifestHelper.ReadFromFile(newManifest);

            ManifestHelper manifestHelperOld = new ManifestHelper();
            manifestHelperOld.ReadFromFile(oldmanifest);

            return manifestHelperOld.DiffNewlyAddedBundles(manifestHelper);
        }

        public List<string> diffAssetVersions(string oldVersion, DiffType diffType = DiffType.All)
        {
            var pathToManifest = GetPathToManifestAndDownloadIfNotExists();
            ManifestHelper manifestHelper = new ManifestHelper();
            manifestHelper.ReadFromFile(pathToManifest);

            var diffManifestUrl = @"https://eaassets-a.akamaihd.net/assetssw.capitalgames.com/PROD/" + oldVersion + this.AssetOSPath;
            var pathToDiffManifest = $"{workingFolder}/Manifest/{oldVersion}_manifest.data";

            using (var client = new WebClient())
            {
                Console.WriteLine($"Downloading DiffManifest");
                client.DownloadFile($"{diffManifestUrl}manifest.data", pathToDiffManifest);
            }

            ManifestHelper manifestHelperOld = new ManifestHelper();
            manifestHelperOld.ReadFromFile(pathToDiffManifest);

            switch (diffType)
            {
                case DiffType.All:
                    return manifestHelperOld.DiffBundles(manifestHelper);
                case DiffType.New:
                    return manifestHelperOld.DiffNewlyAddedBundles(manifestHelper);
                case DiffType.Changed:
                    return manifestHelperOld.DiffChangedBundles(manifestHelper);
                default:
                    return manifestHelperOld.DiffBundles(manifestHelper);
            }
        }

        public void exportSingleFile(string assetName)
        {
            try
            {
                var prefix = assetName.Split('_')[0];
                var downloadedFile = DownloadAssetBundle(assetName);
                fileHelper.UnpackBundle(downloadedFile, $"{targetFolder}/{prefix}", assetName, false, this.exportMeshes, false, false, this.exportSpriteAtlases);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting asset '{assetName}'! you may ignore this.");
                Console.WriteLine(ex);
            }
        }

        public void DownloadManifest()
        {
            Directory.CreateDirectory($"{workingFolder}/Manifest");

            using (var client = new WebClient())
            {
                Console.WriteLine($"Downloading Manifest");
                client.DownloadFile($"{AssetDownloadUrl}manifest.data", $"{workingFolder}/Manifest/{this.AssetVersion}_manifest.data");
            }
        }

        public string DownloadAssetBundle(string assetBundleName)
        {
            try
            {
                Console.WriteLine($"Downloading {assetBundleName}");
                Directory.CreateDirectory($"{workingFolder}/tmp");

                var pathToNewFile = $"{workingFolder}/tmp/{assetBundleName}.bundle";
                using (var client = new WebClient())
                {
                    client.DownloadFile($"{AssetDownloadUrl}{assetBundleName}.bundle", pathToNewFile);
                }
                return pathToNewFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file '{assetBundleName}'! you may ignore this.");
                return null;
            }
        }

        public string DownloadAudioPackages(string assetBundleName)
        {
            try
            {
                Console.WriteLine($"Downloading {assetBundleName}");
                Directory.CreateDirectory($"{workingFolder}/tmp_audio");
                var pathToNewFile = $"{workingFolder}/tmp_audio/{assetBundleName}.wwpkg";
                using (var client = new WebClient())
                {
                    client.DownloadFile($"{AssetDownloadUrl}{assetBundleName}.wwpkg", pathToNewFile);
                }
                return pathToNewFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file '{assetBundleName}'! you may ignore this.");
                return null;
            }
        }

        public string GetPathToManifestAndDownloadIfNotExists()
        {
            var pathToManifest = $"{workingFolder}/Manifest/{this.AssetVersion}_manifest.data";

            if (!File.Exists(pathToManifest))
            {
                DownloadManifest();
            }
            return pathToManifest;
        }

        public List<string> GetAudioFromManifest()
        {
            var pathToManifest = GetPathToManifestAndDownloadIfNotExists();
            ManifestHelper manifestHelper = new ManifestHelper();
            manifestHelper.ReadFromFile(pathToManifest);
            return manifestHelper.audioFiles;
        }

        public List<string> GetAssetsFromManifest()
        {
            var pathToManifest = GetPathToManifestAndDownloadIfNotExists();
            ManifestHelper manifestHelper = new ManifestHelper();
            manifestHelper.ReadFromFile(pathToManifest);
            return manifestHelper.resources;
        }

        public List<string> GetPrefixesFromManifest()
        {
            var pathToManifest = GetPathToManifestAndDownloadIfNotExists();
            ManifestHelper manifestHelper = new ManifestHelper();
            manifestHelper.ReadFromFile(pathToManifest);
            return manifestHelper.prefixes;
        }
    }
}
