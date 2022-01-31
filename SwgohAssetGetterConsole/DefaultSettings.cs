using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SwgohAssetGetterConsole
{
    public class DefaultSettings
    {
        public string workingDirectory { get; set; }
        public string defaultOutputDirectory { get; set; }
        public string defaultAssetVersion { get; set; }

        public static DefaultSettings GetDefaultSettings()
        {
            var settingsJson = File.ReadAllText("settings.json");
            return JsonConvert.DeserializeObject<DefaultSettings>(settingsJson);
        }
    }
}
