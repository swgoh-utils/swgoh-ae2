using AssetGetterTools;
using AssetGetterTools.models;
using System;
using System.IO;

namespace SwgohAssetGetterConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            var mainProgram = new MainProgram();

            var debugMode = false;
            if (debugMode)
            {
                var argumentText = "-downloadDiff 2706 new -v 2708";

                args = argumentText.Split(' ');
            }

            if(args.Length == 0)
            {
                Console.WriteLine("No Parameter was passed.");
                Console.WriteLine($"Usage: ");
                Console.WriteLine("SwgohAssetGetterConsole.exe -downloadManifest");
                Console.WriteLine("SwgohAssetGetterConsole.exe -getAssetNames");
                Console.WriteLine("SwgohAssetGetterConsole.exe -getAssetPrefixes");
                Console.WriteLine("SwgohAssetGetterConsole.exe -single charui_b1");
                Console.WriteLine("SwgohAssetGetterConsole.exe -prefix charui");
                Console.WriteLine("SwgohAssetGetterConsole.exe -audio");
                Console.WriteLine("SwgohAssetGetterConsole.exe -all");
            }
            else
            {
                ProcessAdditionalParameters(args, mainProgram);

                var argToProcess = args[0];
                switch(argToProcess)
                {
                    case "-downloadManifest":
                        Console.WriteLine($"Downloading Manifest...");
                        mainProgram.DownloadManifest();
                        Console.WriteLine($"Manifest downloaded!");
                        break;
                    case "-getAssetNames":
                        Console.WriteLine($"Getting asset Names...");
                        mainProgram.SaveAssetNamesToFile();
                        Console.WriteLine($"Assetnames file created...");
                        break;
                    case "-getAssetPrefixes":
                        Console.WriteLine($"Getting asset Prefixes...");
                        mainProgram.SavePrefixesToFile();
                        Console.WriteLine($"Prefixes file created...");
                        break;
                    case "-single":
                        var assetName = args[1];
                        Console.WriteLine($"Starting exporting assets with name '{assetName}'");
                        mainProgram.exportSingleFile(assetName);
                        break;
                    case "-downloadDiff":
                    case "-dd":
                        var diffVersion = args[1];

                        var diffType = DiffType.New;

                        if (args.Length > 2)
                        {
                            var diffTypeArgument = args[2];

                            switch (diffTypeArgument)
                            {
                                case "all":
                                    Console.WriteLine($"Setting Difftype to {nameof(DiffType.All)}");
                                    diffType = DiffType.All;
                                    break;
                                case "new":
                                    Console.WriteLine($"Setting Difftype to {nameof(DiffType.New)}");
                                    diffType = DiffType.New;
                                    break;
                                case "changed":
                                    Console.WriteLine($"Setting Difftype to {nameof(DiffType.Changed)}");
                                    diffType = DiffType.Changed;
                                    break;
                                default:
                                    Console.WriteLine($"Setting Difftype to {nameof(DiffType.New)}");
                                    diffType = DiffType.New;
                                    break;
                            }
                        }

                        Console.WriteLine($"Starting diffing assetversion {mainProgram.AssetVersion} to {diffVersion}...");
                        var diffs = mainProgram.diffAssetVersions(diffVersion, diffType);
                        Console.WriteLine($"Number of differences: {diffs.Count}");
                        foreach (var diff in diffs)
                        {
                            Console.WriteLine(diff);
                        }
                        Console.WriteLine($"Diff end...");
                        Console.WriteLine($"Start downloading");
                        mainProgram.exportMultipleAssets(diffs);
                        break;
                    case "-prefix":
                        var prefix = args[1];
                        Console.WriteLine($"Starting exporting all assets with prefix '{prefix}'");
                        mainProgram.exportAllWithPrefixFile(prefix);
                        break;
                    case "-all":
                        Console.WriteLine($"Starting exporting all assets");
                        mainProgram.exportAllFiles();
                        break;
                    case "-audio":
                        Console.WriteLine($"Starting exporting all assets");
                        mainProgram.downloadAllAudioFiles();
                        break;
                    default:
                        Console.WriteLine($"Unknown Parameter was passed. Usage: "); 
                        Console.WriteLine("SwgohAssetGetterConsole.exe -downloadManifest");
                        Console.WriteLine("SwgohAssetGetterConsole.exe -getAssetNames");
                        Console.WriteLine("SwgohAssetGetterConsole.exe -getAssetPrefixes");
                        Console.WriteLine("SwgohAssetGetterConsole.exe -single charui_b1");
                        Console.WriteLine("SwgohAssetGetterConsole.exe -prefix charui");
                        Console.WriteLine("SwgohAssetGetterConsole.exe -audio");
                        Console.WriteLine("SwgohAssetGetterConsole.exe -all");
                        break;
                }
            }

            Console.WriteLine("End!");
        }

        private static void ProcessAdditionalParameters(string[] args, MainProgram mainProgram)
        {
            Console.WriteLine("Looking for additional parameters");

            for (var i = 1; i < args.Length; i++)
            {
                var currentArg = args[i];
                if (currentArg.StartsWith('-'))
                {
                    if(i+1 > args.Length)
                    {
                        Console.WriteLine($"There was no input given for '{currentArg}'");
                    }
                    else
                    {
                        var currentParameter = args[i+1];
                        var redownloadManifestAutomation = false;

                        switch (currentArg)
                        {
                            case "-Version":
                            case "-version":
                            case "-V":
                            case "-v":
                                if(mainProgram.AssetVersion != currentParameter)
                                {
                                    redownloadManifestAutomation = true;
                                }
                                mainProgram.AssetVersion = currentParameter;
                                Console.WriteLine($"Assetversion setted to: {mainProgram.AssetVersion}");
                                break;
                            case "-Target":
                            case "-target":
                            case "-T":
                            case "-t":
                                mainProgram.targetFolder = currentParameter;
                                Console.WriteLine($"TargetFolder setted to: {mainProgram.targetFolder}");
                                break;
                            case "-Workingfolder":
                            case "-workingfolder":
                            case "-W":
                            case "-w":
                                mainProgram.workingFolder = currentParameter;
                                Console.WriteLine($"Workingfolder setted to: {mainProgram.workingFolder}");
                                break;
                            case "-ExportMeshes":
                            case "-eM":
                                mainProgram.exportMeshes = (currentParameter == "true" || currentParameter == "t");
                                Console.WriteLine($"exportMeshes setted to: {mainProgram.exportMeshes}");
                                break;
                            default:
                                Console.WriteLine($"Unknown Argument '{currentArg}'");
                                break;
                        }

                        if (redownloadManifestAutomation)
                        {
                            Console.WriteLine($"Redownloading manifestfile because of versionswitch");
                            mainProgram.DownloadManifest();
                        }

                    }
                }
            }

            Console.WriteLine("additional parameters processed");
        }

    }
}
