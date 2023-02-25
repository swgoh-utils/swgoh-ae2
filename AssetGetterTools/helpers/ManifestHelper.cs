using AssetGetterTools.models;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Asset_Getter
{
    public class ManifestHelper
    {
        public RawAssetManifest rawAssetManifest { get; set; }

        public List<string> audioFiles { get; set; }
        public List<string> prefixes { get; set; }
        public List<string> resources { get; set; }

        public List<RawAssetManifestRecord> BundleFiles { get; set; }
        public List<RawAssetManifestRecord> AudioFiles { get; set; }

        public ManifestHelper()
        {
            this.audioFiles = new List<string>();
            this.prefixes = new List<string>();
            this.resources = new List<string>();

            this.BundleFiles = new List<RawAssetManifestRecord>();
            this.AudioFiles = new List<RawAssetManifestRecord>();
        }

        public void ReadFromFile(string filepath)
        {
            using (FileStream inFile = File.OpenRead(filepath))
            {
                rawAssetManifest = Serializer.Deserialize<RawAssetManifest>(inFile);
            }

            BundleFiles = this.rawAssetManifest.Records.Where(record => record.packageType == 0).ToList();
            AudioFiles = this.rawAssetManifest.Records.Where(record => record.packageType == 1 && record.Name != "soundbanksinfo").ToList();

            //backwards compatibility
            var audioFileNames = AudioFiles.Select(audio => audio.Name).ToList();
            this.audioFiles.AddRange(audioFileNames);

            var bundleFileNames = BundleFiles.Select(audio => audio.Name).ToList();
            this.resources.AddRange(bundleFileNames);

            this.audioFiles = audioFiles.Distinct().ToList();
            this.resources = resources.Distinct().ToList();

            foreach (var resource in this.resources)
            {
                var resourcePrefix = resource.Split('_');
                if (resourcePrefix.Length > 1)
                {
                    this.prefixes.Add(resourcePrefix.First());
                }
            }

            this.prefixes = prefixes.Distinct().ToList();

            audioFiles.Sort();
            prefixes.Sort();
            resources.Sort();
        }

        public List<string> DiffNewlyAddedBundles(ManifestHelper newManifestHelper)
        {
            var newlyAddedBundles = new List<string>();

            foreach (var bundleFile in newManifestHelper.BundleFiles)
            {
                var existingbundle = this.BundleFiles.FirstOrDefault(bundle => bundle.Name == bundleFile.Name);
                if (existingbundle == null)
                {
                    newlyAddedBundles.Add(bundleFile.Name);
                }
            }

            return newlyAddedBundles;
        }

        public List<string> DiffBundles(ManifestHelper newManifestHelper)
        {
            var diffBundles = new List<string>();

            foreach (var bundleFile in newManifestHelper.BundleFiles)
            {
                var existingbundle = this.BundleFiles.FirstOrDefault(bundle => bundle.Name == bundleFile.Name);
                if (existingbundle == null)
                {
                    diffBundles.Add(bundleFile.Name);
                }
                else
                {
                    if (existingbundle.Version != bundleFile.Version)
                    {
                        diffBundles.Add(bundleFile.Name);
                    }
                }
            }

            return diffBundles;
        }

        public List<string> DiffChangedBundles(ManifestHelper newManifestHelper)
        {
            var diffChangedBundles = new List<string>();

            foreach (var bundleFile in newManifestHelper.BundleFiles)
            {
                var existingbundle = this.BundleFiles.FirstOrDefault(bundle => bundle.Name == bundleFile.Name);
                if (existingbundle != null && existingbundle.Version != bundleFile.Version)
                {
                    diffChangedBundles.Add(bundleFile.Name);
                }
            }

            return diffChangedBundles;
        }
    }
}
