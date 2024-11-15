﻿using Asset_Getter;
using AssetGetterTools;
using AssetGetterTools.models;
using AssetGetterTools.pck;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AssetGUI
{
    public class MainProgram
    {
        public string workingFolder { get; set; }
        public string targetFolder { get; set; }
        public string audioTargetFolder { get; set; }
        public string AssetVersion { get; set; }

        public bool exportShader { get; set; }
        public bool exportMeshes { get; set; }
        public bool exportAnimator { get; set; }

        public string AssetDownloadUrl
        {
            get
            {
                return @"https://eaassets-a.akamaihd.net/assetssw.capitalgames.com/PROD/" + this.AssetVersion + this.AssetOSPath;
            }
        }
        public string AssetOSPath { get; set; }

        public Filehelper fileHelper { get; set; }

        public BarbPCKExtractor barbPCKExtractor { get; set; }

        public MainProgram(AssetOS assetOS = AssetOS.Windows)
        {
            this.fileHelper = new Filehelper();
            this.barbPCKExtractor = new BarbPCKExtractor();

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

        public async Task<bool> exportAllFiles()
        {
            var allAssets = GetAssetsFromManifest();
            exportMultipleAssets(allAssets);
            Console.WriteLine($"Done exportAllFiles!");
            return true;
        }

        public async Task<bool> downloadAllAudioFiles()
        {
            var allAssets = GetAudioFromManifest();
            foreach (var asset in allAssets)
            {
                var audioWwpkgFile = DownloadAudioPackages(asset);
                var extractedPCK = ExtractAudioPackage(audioWwpkgFile, asset);
                if(extractedPCK != null)
                {
                    ConvertPCKToWave(extractedPCK);
                }
            }
            Console.WriteLine("done downloading all Audio files");
            return true;
        }

        public async Task<bool> exportAllWithPrefixFile(string prefix)
        {
            var allAssetsWithPrefix = GetAssetsFromManifest().Where(r => r.StartsWith(prefix)).ToList();
            foreach (var asset in allAssetsWithPrefix)
            {
                exportSingleFile(asset);
            }

            Console.WriteLine($"Done exportAllWithPrefixFile!");
            return true;
        }

        public void exportMultipleAssets(List<string> allAssetsToExport)
        {
            foreach (var asset in allAssetsToExport)
            {
                exportSingleFile(asset);
            }
            Console.WriteLine($"Done exportMultipleAssets!");
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

        public async Task<bool> exportSingleFile(string assetName)
        {
            try
            {
                var prefix = assetName.Split('_')[0];
                var downloadedFile = DownloadAssetBundle(assetName);
                fileHelper.UnpackBundle(downloadedFile, $"{targetFolder}/{prefix}", assetName, this.exportShader, this.exportMeshes, this.exportAnimator);
                Console.WriteLine($"Done exportSingleFile!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting asset '{assetName}'! you may ignore this.");
                Console.WriteLine(ex);
            }

            return true;
        }

        public void DownloadManifest()
        {
            Directory.CreateDirectory($"{workingFolder}/Manifest");

            try
            {
                using (var client = new WebClient())
                {
                    Console.WriteLine($"Downloading Manifest");
                    var downloadedManifestUrl = $"{workingFolder}/Manifest/{this.AssetVersion}_manifest.data";
                    client.DownloadFile($"{AssetDownloadUrl}manifest.data", downloadedManifestUrl);
                    Console.WriteLine($"Done downloading Manifest");

                    using (FileStream inFile = File.OpenRead(downloadedManifestUrl))
                    {
                        Console.WriteLine($"Writing Json Manifest");

                        var rawAssetManifest = Serializer.Deserialize<RawAssetManifest>(inFile);
                        var jsonRawAssetManifest = JsonConvert.SerializeObject(rawAssetManifest);
                        var downloadedManifestJsonPath = $"{workingFolder}/Manifest/{this.AssetVersion}_manifest.json";

                        File.WriteAllText(downloadedManifestJsonPath, jsonRawAssetManifest);

                        Console.WriteLine($"Done writing json Manifest");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading Manifest");
                throw;
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

        public void ConvertPCKToWave(string pckFileToExtract)
        {
            try
            {
                Console.WriteLine($"Converting {Path.GetFileName(pckFileToExtract)}");
                var audioOutputDirectory = audioTargetFolder;
                Directory.CreateDirectory(audioOutputDirectory);

                this.barbPCKExtractor.ExtractSingleFile(pckFileToExtract, audioOutputDirectory);
                Console.WriteLine($"Done Converting!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting file '{pckFileToExtract}'! you may ignore this.");
            }
        }

        public string ExtractAudioPackage(string audioWwwpkgPath, string assetBundleName)
        {
            try
            {
                Console.WriteLine($"Extracting {assetBundleName}");
                Directory.CreateDirectory($"{workingFolder}/tmp_audio_pck");

                var audioZipFile = ZipFile.Open(audioWwwpkgPath, ZipArchiveMode.Read);

                var pathToNewFile = $"{workingFolder}/tmp_audio_pck/{assetBundleName}.pck";

                var pckFile = audioZipFile.Entries.FirstOrDefault(entry => entry.Name.EndsWith("pck"));

                if(pckFile == null)
                {
                    Console.WriteLine($"Cant Extract {assetBundleName} - this may have different reasons");
                    return null;
                }

                pckFile.ExtractToFile(pathToNewFile);

                return pathToNewFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting file '{assetBundleName}'! you may ignore this.");
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
