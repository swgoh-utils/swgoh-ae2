using AssetGetterTools.models;
using Microsoft.AspNetCore.Mvc;
using AssetWebApi.Helpers;

namespace AssetWebApi.Controllers
{
    /// <summary>
    /// Endpoints for swgoh assets. 
    /// 
    /// 
    /// AssetOS Enum:
    /// Windows = 0
    /// Android = 1
    /// iOS = 2
    /// 
    /// DiffType Enum
    /// All = 0
    /// New = 1
    /// Changed = 2
    /// 
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly ILogger<AssetController> _logger;

        public AssetController(ILogger<AssetController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Lists all possible Assets for a specific version
        /// </summary>
        /// <param name="version">the assetversion. you can get it via Metadata in comlink.</param>
        /// <returns></returns>
        [HttpGet("list")]
        public IEnumerable<string> Get(int version, AssetOS assetOS = AssetOS.Windows)
        {
            var mainProgram = new MainProgram(assetOS);
            mainProgram.AssetVersion = version.ToString();
            return mainProgram.GetAssetsFromManifest();
        }

        /// <summary>
        /// Lists all possible Assets for a specific version
        /// </summary>
        /// <param name="version">the assetversion. you can get it via Metadata in comlink.</param>
        /// <param name="diffVersion">the assetversion to diff. This is usually the older version</param>
        /// <param name="diffType">Says how to diff the assetversions. Defaults to "All" wich lists Newly added and Changed assets</param>
        /// <param name="prefix">Filtery by prefix. For example "charui" gives only character images</param>
        /// <returns></returns>
        [HttpGet("listDiff")]
        public IEnumerable<string> listDiff(int version, int diffVersion, DiffType diffType = DiffType.All, string? prefix = null, AssetOS assetOS = AssetOS.Windows)
        {
            var mainProgram = new MainProgram(assetOS);
            mainProgram.AssetVersion = version.ToString();
            return mainProgram.diffAssetVersions(diffVersion.ToString(), diffType, prefix); ;
        }

        /// <summary>
        /// Gets a Texture asset (image) for a given Name
        /// </summary>
        /// <param name="assetName">the name of the asset to download</param>
        /// <param name="version">the assetversion. you can get it via Metadata in comlink.</param>
        /// <param name="forceReDownload">Optional parameter (default = false). true Forces a re-download from the CG Server. Otherwise it uses the cache if possible.</param>
        /// <returns></returns>
        [HttpGet("single")]
        public FileContentResult Get(string assetName, int version, bool forceReDownload = false, AssetOS assetOS = AssetOS.Windows)
        {
            var mainProgram = new MainProgram(assetOS);
            mainProgram.AssetVersion = version.ToString();
            var singleFilePath = mainProgram.getSingleTextureIfExists(assetName, forceReDownload);
            var fileContent = System.IO.File.ReadAllBytes(singleFilePath);
            return File(fileContent, "application/octet-stream", Path.GetFileName(singleFilePath));
        }

        /// <summary>
        /// Gets a bundle of Textures and returns them as a zip. May contain multiple.
        /// </summary>
        /// <param name="assetName">the name of the asset to download</param>
        /// <param name="version">the assetversion. you can get it via Metadata in comlink.</param>
        /// <param name="exportSpriteAtlases">Optional parameter (default = false). true will return sprite atlases</param>
        /// <returns></returns>
        [HttpGet("zip")]
        public FileStreamResult GetZip(string assetName, int version, bool exportSpriteAtlases = false, AssetOS assetOS = AssetOS.Windows)
        {
            var defaultSettings = DefaultSettings.GetDefaultSettings() ?? 
                throw new Exception("Could not load DefaultSettings. settings.json may not exist");
            using (var tempWorkingFolder = new TempFolder(defaultSettings.defaultOutputDirectory))
            {
                var mainProgram = new MainProgram(assetOS);
                mainProgram.AssetVersion = version.ToString();
                mainProgram.targetFolder = tempWorkingFolder.Folder;
                mainProgram.exportSpriteAtlases = exportSpriteAtlases;
                mainProgram.getSingleTextureIfExists(assetName);

                var zipStream = tempWorkingFolder.ToZipStream();

                return File(zipStream, "application/zip", $"{assetName}.zip");
            }
        }
    }
}