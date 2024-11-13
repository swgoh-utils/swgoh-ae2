// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Cache;
using System.Net;
using Asset_Getter;
using System.Runtime.InteropServices;
using AssetGetterTools.models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AssetGUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private string version = "3.0";
        private string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assets_swgoh");
        private MainProgram mainProgram;
        private IList<DiffType> _diffTypes = Enum.GetValues(typeof(DiffType)).Cast<DiffType>().ToList();
        public IList<DiffType> DiffTypes => _diffTypes;
        public DiffType SelectedDiffType { get; set; }
        public List<AssetOS> AssetOSs { get; set; }
        public AssetOS SelectedAssetOS { get; set; }
        public List<string> DownloadableAssets { get; set; }
        public string SelectedDownloadableAsset { get; set; }
        public List<string> prefixes { get; set; }
        public string SelectedPrefix { get; set; }

        public MainWindow()
        {
            //Microsoft.UI.Xaml.Controls.TextBlock
            this.InitializeComponent();
            AllocConsole();
            //Console.SetOut();

            this.mainProgram = new MainProgram();
            SetWindowSize(800, 500);
            setVisibilityOfSecondaryRows(false);

            this.AssetOSs = new List<AssetOS>() { AssetOS.Windows, AssetOS.Android, AssetOS.iOS };
            this.SelectedAssetOS = AssetOS.Windows;
            this.DownloadableAssets = new List<string>();
            this.prefixes = new List<string>();

            this.SelectedDiffType = DiffType.New;
            this.tbExportPath.Text = this.path;
            Console.WriteLine("Assetgetter Started. Current Version: " + version);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void btSetExportPath_Click(object sender, RoutedEventArgs e)
        {
            this.setPathViaPicker();
        }

        private void btRefreshVersion_Click(object sender, RoutedEventArgs e)
        {
            this.refreshVersion();
        }

        private void btGetManifest_Click(object sender, RoutedEventArgs e)
        {
            getListOfAssets();
        }

        private void btDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => downloadAll());
        }

        private void btDownloadSingle_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => downloadSingle());
        }

        private void btDownloadPrefix_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => downloadPrefixes());
        }

        private void btDownloadDiff_Click(object sender, RoutedEventArgs e)
        {
            var oldVersion = tbDownloadDiff.Text;
            Task.Run(() => downloadDiff(oldVersion));
        }

        private void btDownloadAudio_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => downloadAudio());
        }

        private async void downloadAudio()
        {
            await mainProgram.downloadAllAudioFiles();
        }

        private async void downloadSingle()
        {
            await mainProgram.exportSingleFile(SelectedDownloadableAsset);
        }

        private async void downloadPrefixes()
        {
            await mainProgram.exportAllWithPrefixFile(SelectedPrefix);
        }

        private async void downloadAll()
        {
            await mainProgram.exportAllFiles();
        }

        private async Task<bool> downloadDiff(string oldVersion)
        {
            var diffs = mainProgram.diffAssetVersions(oldVersion, SelectedDiffType);
            Console.WriteLine($"Number of differences: {diffs.Count}");
            foreach (var diff in diffs)
            {
                Console.WriteLine(diff);
            }
            Console.WriteLine($"Diff end...");
            Console.WriteLine($"Start downloading");
            mainProgram.exportMultipleAssets(diffs);
            Console.WriteLine($"Done downloading");
            return true;
        }

        private async void getListOfAssets()
        {
            mainProgram.AssetVersion = tbVersion.Text;
            mainProgram.workingFolder = tbExportPath.Text;
            mainProgram.targetFolder = tbExportPath.Text + "/OutPut";
            mainProgram.audioTargetFolder = this.tbExportPath.Text + "/Audio_OutPut";

            this.mainProgram.SetAssetOSPath(this.SelectedAssetOS);

            Directory.CreateDirectory(mainProgram.workingFolder);
            Directory.CreateDirectory(mainProgram.targetFolder);

            mainProgram.DownloadManifest();

            DownloadableAssets.AddRange(mainProgram.GetAssetsFromManifest());
            SelectedDownloadableAsset = DownloadableAssets.FirstOrDefault();
            prefixes.AddRange(mainProgram.GetPrefixesFromManifest());
            SelectedPrefix = prefixes.FirstOrDefault();

            SetWindowSize(800, 500);
            setVisibilityOfSecondaryRows(true);
        }

        private void setVisibilityOfSecondaryRows(bool visible)
        {
            GridLength gridLength = new GridLength(0);

            if (visible)
            {
                gridLength = new GridLength(35);
            }
            else
            {
                gridLength = new GridLength(0);
            }

            rowDownloadSingle.Height = gridLength;
            rowDownloadPrefix.Height = gridLength;
            rowDownloadAll.Height = gridLength;
            rowDownloadDiff.Height = gridLength;
            rowDownloadAudio.Height = gridLength;
        }

        private async void setPathViaPicker()
        {
            FolderPicker folderPicker = new();
            folderPicker.FileTypeFilter.Add("*");

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);


            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            tbExportPath.Text = folder.Path;

            mainProgram.workingFolder = this.tbExportPath.Text;
            mainProgram.targetFolder = this.tbExportPath.Text + "/OutPut";
            mainProgram.audioTargetFolder = this.tbExportPath.Text + "/Audio_OutPut";
        }

        private async void refreshVersion()
        {
            Console.WriteLine("Trying to get newest active Assetversion");
            var versionGetterUrl = "https://swgoh-guild-commander.azurewebsites.net/SwgohMain/AssetVersion";
            var versionResponse = await new HttpClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, versionGetterUrl));
            if (versionResponse.IsSuccessStatusCode)
            {
                var versionResult = await versionResponse.Content.ReadAsStringAsync();
                Console.WriteLine("AssetVersion: " + versionResult);
                this.tbVersion.Text = versionResult;
            }
            else
                Console.WriteLine(string.Format("Error getting newest Assetversion. Requeststatus: {0}", (object)versionResponse.StatusCode));
        }

        private void SetWindowSize(int width, int height)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(StartWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            var size = new SizeInt32(width, height);
            mainGrid.Width = size.Width - 25;
            appWindow.Resize(size);
        }

        private void cbShader_Checked(object sender, RoutedEventArgs e)
        {
            this.mainProgram.exportShader = this.cbShader.IsChecked.Value;
        }

        private void cbMeshes_Checked(object sender, RoutedEventArgs e)
        {
            this.mainProgram.exportMeshes = this.cbMeshes.IsChecked.Value;
        }

        private void cbAnimator_Checked(object sender, RoutedEventArgs e)
        {
            this.mainProgram.exportAnimator = this.cbAnimator.IsChecked.Value;
        }
    }
}
