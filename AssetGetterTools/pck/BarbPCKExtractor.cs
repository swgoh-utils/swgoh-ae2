using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetGetterTools.pck
{
    public class BarbPCKExtractor
    {
        public void ExtractSingleFile(string fileToExtract, string outputFolder)
        {
            var dllDir = GetDirectedDllDirectory();


            var inputFileName = Path.GetFileName(fileToExtract);

            var unpackFileName = inputFileName.Replace(".pck", "");

            var fullOutputPath = Path.Combine(outputFolder, unpackFileName);
            Directory.CreateDirectory(fullOutputPath);

            /*
             * macht aus einer PCK mehrere wav
             * quickbms.exe wavescan.bms "Game Files" "Tools\Decoding"
             * 
             * macht aus wav eine OGG:
             * ww2ogg.exe "C:\Users\midi.D1\Documents\swgoh\Git\Wwise-Unpacker\tools\Decoding\audio_mus_home_1.wav" --pcb Tools\packed_codebooks_aoTuV_603.bin
             * 
             * macht aus OGG eine MP3:
             * revorb.exe "C:\Users\midi.D1\Documents\swgoh\Git\Wwise-Unpacker\tools\Decoding\audio_mus_home_1.ogg"
             */

            var pathToExecuteable_quickbms = $"{dllDir}/PCK_Tools_Native_win/quickbms.exe";
            var pathToExecuteable_ww2ogg = $"{dllDir}/PCK_Tools_Native_win/ww2ogg.exe";
            var pathToExecuteable_revorb = $"{dllDir}/PCK_Tools_Native_win/revorb.exe";

            var pathTobms_wavescan = $"{dllDir}/PCK_Tools_Native_win/wavescan.bms";

            var pathTobms_packed_codebooks_aoTuV_603 = $"{dllDir}/PCK_Tools_Native_win/packed_codebooks_aoTuV_603.bin";

            this.CheckIfExecuteableIsAvailable(pathToExecuteable_quickbms);
            this.CheckIfExecuteableIsAvailable(pathToExecuteable_ww2ogg);
            this.CheckIfExecuteableIsAvailable(pathToExecuteable_revorb);

            this.CheckIfExecuteableIsAvailable(pathTobms_wavescan);

            this.CheckIfExecuteableIsAvailable(pathTobms_packed_codebooks_aoTuV_603);

            var process = new Process();
            process.StartInfo.WorkingDirectory = fullOutputPath;
            process.StartInfo.FileName = pathToExecuteable_quickbms;
            process.StartInfo.Arguments = $"{pathTobms_wavescan} {fileToExtract} {fullOutputPath}";
            process.Start();
            process.WaitForExit();

            var allWavFilesInOutputFolder = Directory.GetFiles(fullOutputPath).Where(filename => filename.EndsWith(".wav")).ToList();

            if (allWavFilesInOutputFolder.Count > 0)
            {
                //cast into OGG
                foreach (var currentFile in allWavFilesInOutputFolder)
                {
                    var filename = Path.GetFileName(currentFile);
                    var filePath = Path.GetDirectoryName(currentFile);

                    var processWW2OGG = new Process();
                    processWW2OGG.StartInfo.WorkingDirectory = fullOutputPath;
                    processWW2OGG.StartInfo.FileName = pathToExecuteable_ww2ogg;
                    processWW2OGG.StartInfo.Arguments = $"{currentFile} --pcb {pathTobms_packed_codebooks_aoTuV_603}";
                    processWW2OGG.Start();
                    processWW2OGG.WaitForExit();

                    File.Delete(currentFile);
                }

                var allOGGFilesInOutputFolder = Directory.GetFiles(fullOutputPath).Where(filename => filename.EndsWith(".ogg")).ToList();

                //Cast to MP3
                foreach (var currentOGGFile in allOGGFilesInOutputFolder)
                {
                    var filename = Path.GetFileName(currentOGGFile);
                    var filePath = Path.GetDirectoryName(currentOGGFile);

                    var processWW2OGG = new Process();
                    processWW2OGG.StartInfo.WorkingDirectory = fullOutputPath;
                    processWW2OGG.StartInfo.FileName = pathToExecuteable_revorb;
                    processWW2OGG.StartInfo.Arguments = currentOGGFile;
                    processWW2OGG.Start();
                    processWW2OGG.WaitForExit();

                    File.Move(currentOGGFile, currentOGGFile.Replace(".ogg", ".mp3"));
                }
            }

            //Delete all Subfolder
            var allSubfolders = Directory.GetDirectories(fullOutputPath);
            foreach(var subfolderNow in allSubfolders)
            {
                Directory.Delete(subfolderNow);
            }
        }

        private void CheckIfExecuteableIsAvailable(string pathToExecutable)
        {
            Console.WriteLine($"Checking for file {pathToExecutable}");

            if (!File.Exists(pathToExecutable))
            {
                throw new Exception($"The Executable could not be found. Make sure it exists in the folder '{pathToExecutable}'");
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
