using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AssetStudio;
using AssetStudio.PInvoke;
using AssetStudioGUI;

namespace AssetGetterTools
{
    public class Filehelper
    {
        public string workingFolder { get; set; }
        public static List<AssetItem> exportableAssets = new List<AssetItem>();

        public Filehelper()
        {
            verifytextureDLLisReady();
        }

        public void UnpackBundle(string inFile, string targetFolder, string assetName, bool exportShader = false, bool exportMeshes = false, bool exportAnimator = false, bool exportMonoBehavior = false)
        {
            Directory.CreateDirectory(targetFolder);

            var pathes = new List<string>();
            pathes.Add(inFile);

            var assetManager = new AssetsManager();
            exportableAssets.Clear();

            assetManager.LoadFiles(pathes.ToArray());

            foreach (var assetsFile in assetManager.assetsFileList)
            {
                foreach (var asset in assetsFile.Objects)
                {
                    var assetItem = new AssetItem(asset);
                    var exportable = false;
                    switch (asset)
                    {
                        case GameObject m_GameObject:
                            assetItem.Text = m_GameObject.m_Name;
                            break;
                        case Texture2D m_Texture2D:
                            if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                                assetItem.FullSize = asset.byteSize + m_Texture2D.m_StreamData.size;
                            assetItem.Text = m_Texture2D.m_Name;
                            exportable = true;
                            break;
                        case AudioClip m_AudioClip:
                            if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                assetItem.FullSize = asset.byteSize + m_AudioClip.m_Size;
                            assetItem.Text = m_AudioClip.m_Name;
                            exportable = true;
                            break;
                        case VideoClip m_VideoClip:
                            if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                assetItem.FullSize = asset.byteSize + (long)m_VideoClip.m_ExternalResources.m_Size;
                            assetItem.Text = m_VideoClip.m_Name;
                            exportable = false;
                            break;
                        case Shader m_Shader:
                            assetItem.Text = m_Shader.m_ParsedForm?.m_Name ?? m_Shader.m_Name;
                            exportable = exportShader;
                            break;
                        case Mesh _:
                        case TextAsset _:
                        case AnimationClip _:
                        case Font _:
                        case MovieTexture _:
                        case Sprite _:
                            assetItem.Text = ((NamedObject)asset).m_Name;
                            exportable = exportMeshes;
                            break;
                        case Animator m_Animator:
                            if (m_Animator.m_GameObject.TryGet(out var gameObject))
                            {
                                assetItem.Text = gameObject.m_Name;
                            }
                            exportable = exportAnimator;
                            break;
                        case MonoBehaviour m_MonoBehaviour:
                            if (m_MonoBehaviour.m_Name == "" && m_MonoBehaviour.m_Script.TryGet(out var m_Script))
                            {
                                assetItem.Text = m_Script.m_ClassName;
                            }
                            else
                            {
                                assetItem.Text = m_MonoBehaviour.m_Name;
                            }
                            exportable = exportMonoBehavior;
                            break;
                        case NamedObject m_NamedObject:
                            assetItem.Text = m_NamedObject.m_Name;
                            break;
                    }
                    if (assetItem.Text == "")
                    {
                        assetItem.Text = assetItem.TypeString + assetItem.UniqueID;
                    }
                    if (exportable)
                    {
                        exportableAssets.Add(assetItem);
                    }
                }
            }

            foreach (var exportAbleAsset in exportableAssets)
            {
                var result = Exporter.ExportConvertFile(exportAbleAsset, $"{targetFolder}");
            }

        }

        public void verifytextureDLLisReady()
        {
            var dllDir = GetDirectedDllDirectory();

            var fulFileName = $"{dllDir}\\Texture2DDecoderNative.dll";

            Console.WriteLine($"Checking for file {fulFileName}");

            if (!File.Exists(fulFileName))
            {
                throw new Exception($"The File Texture2DDecoderNative.dll could not be found. Make sure it exists in the folder '{dllDir}'");
            }
        }

        private static string GetDirectedDllDirectory()
        {
            var localPath = Process.GetCurrentProcess().MainModule.FileName;
            var localDir = Path.GetDirectoryName(localPath);

            var subDir = Environment.Is64BitProcess ? "x64" : "x86";

            var directedDllDir = Path.Combine(localDir, subDir);

            return directedDllDir;
        }

    }
}
