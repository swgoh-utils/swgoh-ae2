using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetGetterTools.models;

namespace AssetGetterTools.helpers
{
    public static class SpriteAtlasParser
    {
        public static Atlas ParseAtlas(byte[] data)
        {
            var atlas = new Atlas();

            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                // PPtr<GameObject>
                int fileID_GameObject = br.ReadInt32();
                long pathID_GameObject = br.ReadInt64();

                // m_Enabled
                bool mEnabled = br.ReadBoolean();
                Align4(br);

                // PPtr<Script>
                int fileID_Script = br.ReadInt32();
                long pathID_Script = br.ReadInt64();

                // m_Name
                int nameLen = br.ReadInt32();
                string atlasName = Encoding.UTF8.GetString(br.ReadBytes(nameLen));
                Align4(br);
                atlas.Name = atlasName;

                // PPtr<Material>
                int fileID_Material = br.ReadInt32();
                long pathID_Material = br.ReadInt64();

                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    SpriteItem sprite = new SpriteItem();

                    int spriteNameLen = br.ReadInt32();
                    sprite.Name = Encoding.UTF8.GetString(br.ReadBytes(spriteNameLen));
                    Align4(br);

                    sprite.X = br.ReadInt32();
                    sprite.Y = br.ReadInt32();
                    sprite.Width = br.ReadInt32();
                    sprite.Height = br.ReadInt32();
                    sprite.BorderLeft = br.ReadInt32();
                    sprite.BorderRight = br.ReadInt32();
                    sprite.BorderTop = br.ReadInt32();
                    sprite.BorderBottom = br.ReadInt32();
                    sprite.PaddingLeft = br.ReadInt32();
                    sprite.PaddingRight = br.ReadInt32();
                    sprite.PaddingTop = br.ReadInt32();
                    sprite.PaddingBottom = br.ReadInt32();
                    sprite.MirrorHorizontal = br.ReadBoolean();
                    Align4(br);
                    sprite.MirrorVertical = br.ReadBoolean();
                    Align4(br);
                    sprite.MirrorRotate = br.ReadBoolean();
                    Align4(br);

                    atlas.Sprites.Add(sprite);
                }
            }

            return atlas;
        }

        static void Align4(BinaryReader br)
        {
            long pos = br.BaseStream.Position;
            long mod = pos % 4;
            if (mod != 0)
                br.BaseStream.Position += (4 - mod);
        }
    }
    }

