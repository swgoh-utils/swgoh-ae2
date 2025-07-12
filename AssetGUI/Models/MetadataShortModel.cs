using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetGUI.Models
{
    public class MetadataShortModel
    {
        public MetadataData data { get; set; }
    }

    public class MetadataData
    {
        public int assetVersion { get; set; }
    }
}
