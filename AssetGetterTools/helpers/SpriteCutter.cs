using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetGetterTools.models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AssetGetterTools.helpers
{
    public class SpriteCutter
    {
        public static void CutSprites(Atlas atlas, string exportPath)
        {
            if (TryExportFile(exportPath, atlas.Name, ".png", out var loadFullPath)) // Image doesn't exist
            {
                Console.WriteLine($"Could not export sprite atlas: {atlas.Name}. You can ignore this");
                return;
            }

            Image<Rgba32> SpriteImage = Image.Load<Rgba32>(loadFullPath);
            foreach (var sprite in atlas.Sprites)
            {
                Image<Rgba32> spriteOutput = SpriteImage.Clone(img => img.Crop(new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height)));
                if (!TryExportFile(Path.Combine(exportPath, "sprites", atlas.Name), sprite.Name, ".png", out var exportFullPath))
                    continue;
                spriteOutput.Save(exportFullPath);
            }
        }

        private static bool TryExportFile(string dir, string itemName, string extension, out string fullPath)
        {
            var fileName = FixFileName(itemName);
            fullPath = Path.Combine(dir, fileName + extension);
            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(dir);
                return true;
            }
            return false;
        }

        private static string FixFileName(string str)
        {
            if (str.Length >= 260) return Path.GetRandomFileName();
            return Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c, '_'));
        }
    }
}
