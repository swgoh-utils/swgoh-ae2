using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetGetterTools.models
{
    public class SpriteItem
    {
        public string Name;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int BorderLeft;
        public int BorderRight;
        public int BorderTop;
        public int BorderBottom;
        public int PaddingLeft;
        public int PaddingRight;
        public int PaddingTop;
        public int PaddingBottom;
        public bool MirrorHorizontal;
        public bool MirrorVertical;
        public bool MirrorRotate;
    }

    public class Atlas
    {
        public string Name;
        public List<SpriteItem> Sprites = new List<SpriteItem>();
    }
}
