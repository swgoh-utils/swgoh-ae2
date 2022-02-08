using Microsoft.AspNetCore.Mvc;

namespace AssetWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly ILogger<AssetController> _logger;

        public AssetController(ILogger<AssetController> logger)
        {
            _logger = logger;
        }

        [HttpGet("list")]
        public IEnumerable<string> Get(int version)
        {
            var mainProgram = new MainProgram();
            mainProgram.AssetVersion = version.ToString();
            return mainProgram.GetAssetsFromManifest();
        }

        [HttpGet("single")]
        public FileContentResult Get(string assetName, int version, bool forceReDownload = false)
        {
            var mainProgram = new MainProgram();
            mainProgram.AssetVersion = version.ToString();
            var singleFilePath = mainProgram.getSingleTextureIfExists(assetName, forceReDownload);
            var fileContent = System.IO.File.ReadAllBytes(singleFilePath);
            return File(fileContent, "application/octet-stream", Path.GetFileName(singleFilePath));
        }
    }
}