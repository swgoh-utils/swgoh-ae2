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

namespace AssetWebApi
{
    public class MainProgram
    {
        public string workingFolder { get; set; }
        public string targetFolder { get; set; }
        public string AssetVersion { get; set; }

        public bool exportMeshes { get; set; }

        public string AssetDownloadUrl
        {
            get
            {
                return @"https://eaassets-a.akamaihd.net/assetssw.capitalgames.com/PROD/" + this.AssetVersion + @"/Android/ETC/";
            }
        }

        public Filehelper fileHelper { get; set; }

        public MainProgram()
        {
            var defaultSettings = DefaultSettings.GetDefaultSettings();

            this.workingFolder = defaultSettings.workingDirectory;
            this.targetFolder = defaultSettings.defaultOutputDirectory;
            this.AssetVersion = defaultSettings.defaultAssetVersion;
            this.exportMeshes = defaultSettings.exportMeshes;

            this.fileHelper = new Filehelper();
            this.fileHelper.workingFolder = defaultSettings.workingDirectory;
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

        public List<string> diffAssetVersions(string oldVersion, DiffType diffType = DiffType.All)
        {
            var pathToManifest = GetPathToManifestAndDownloadIfNotExists();
            ManifestHelper manifestHelper = new ManifestHelper();
            manifestHelper.ReadFromFile(pathToManifest);

            var diffManifestUrl = @"https://eaassets-a.akamaihd.net/assetssw.capitalgames.com/PROD/" + oldVersion + @"/Android/ETC/";
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

        public string getSingleTextureIfExists(string assetName, bool forceReDownload = false)
        {
            try
            {
                var prefix = assetName.Split('_')[0];
                var fullFilePath = $"{targetFolder}/{prefix}/tex.{assetName}.png";

                if (File.Exists(fullFilePath) && !forceReDownload)
                {
                    return fullFilePath;
                }
                else
                {
                    var downloadedFile = DownloadAssetBundle(assetName);
                    fileHelper.UnpackBundle(downloadedFile, $"{targetFolder}/{prefix}", assetName, false, this.exportMeshes);
                    return fullFilePath;
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting asset '{assetName}'! you may ignore this.");
                Console.WriteLine(ex);
            }
            return null;
        }

        public string exportSingleFile(string assetName)
        {
            try
            {
                var prefix = assetName.Split('_')[0];
                var downloadedFile = DownloadAssetBundle(assetName);
                fileHelper.UnpackBundle(downloadedFile, $"{targetFolder}/{prefix}", assetName, false, this.exportMeshes);
                return $"{targetFolder}/{prefix}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting asset '{assetName}'! you may ignore this.");
                Console.WriteLine(ex);
            }
            return targetFolder;
        }

        public void DownloadManifest()
        {
            Directory.CreateDirectory($"{workingFolder}/Manifest");

            using (var client = new WebClient())
            {
                Console.WriteLine($"Downloading Manifest");
                client.DownloadFile($"{AssetDownloadUrl}manifest.data", $"{workingFolder}/Manifest/manifest.data");
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
            var pathToManifest = $"{workingFolder}/Manifest/manifest.data";

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
