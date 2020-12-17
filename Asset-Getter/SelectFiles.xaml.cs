using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Converters;
using uTinyRipperGUI.Exporters;

namespace Asset_Getter
{
    /// <summary>
    /// Interaktionslogik für SelectFiles.xaml
    /// </summary>
    public partial class SelectFiles : Window
    {
        public ManifestHelper manifestHelper { get; set; }
        public string path { get; set; }
        public string assetUrl { get; set; }

        private List<string> allFilesToDownload { get; set; }
        private Thread thread { get; set; }

        public static bool AssetSelector(uTinyRipper.Classes.Object asset)
        {
            return true;
        }
        public static bool Texture2DAssetSelector(uTinyRipper.Classes.Object asset)
        {
            return asset.AssetInfo.ClassID == ClassIDType.Texture2D;
        }

        public SelectFiles(ManifestHelper manifestHelper, string path, string assetUrl)
        {
            this.manifestHelper = manifestHelper;
            this.path = path;
            this.assetUrl = assetUrl;

            InitializeComponent();

            cbSingleFile.ItemsSource = this.manifestHelper.resources;
            cbMulti.ItemsSource = this.manifestHelper.prefixes;
            this.Show();
        }

        private void BtSingleFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DownloadSingleFile(cbSingleFile.SelectedValue.ToString());
                MessageBox.Show($"Completed. Look under {this.path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while Downloading the file. Message: {ex.Message}");
            }
        }

        private void BtMulti_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if(thread != null && thread.IsAlive)
                {
                    MessageBox.Show($"Wait for old Thread to finish first");
                    return;
                }
                
                var currentprefix = cbMulti.SelectedValue.ToString();

                AllocConsole();
                Console.WriteLine($"Starting to download all files with prefix '{currentprefix}'");

                allFilesToDownload = this.manifestHelper.resources.Where(r => r.StartsWith(currentprefix)).ToList();

                thread = new Thread(new ThreadStart(DownloadMass));
                thread.Start();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while Downloading the files. Message: {ex.Message}");
            }
        }

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public void DownloadMass()
        {
            var allCount = allFilesToDownload.Count;
            Console.WriteLine($"Identified {allCount} files to download");

            for (var i = 0; i < allCount; i++)
            {
                Console.WriteLine($"Downloading file {i+1}/{allCount}");
                this.DownloadSingleFile(allFilesToDownload[i]);
            }
            
            MessageBox.Show($"Completed. Look under {this.path}");
        }

        private void DownloadSingleFile(string filename)
        {
            try
            {
                Directory.CreateDirectory($"{path}/tmp/{filename}");

                using (var client = new WebClient())
                {
                    client.DownloadFile($"{assetUrl}{filename}.bundle", $"{path}/tmp/{filename}/{filename}.bundle");
                }
                List<string> pathes = new List<string>();
                pathes.Add($"{path}/tmp/{filename}/{filename}.bundle");
                var gameStructure = GameStructure.Load(pathes);
                //EngineAssetExporter engineExporter = new EngineAssetExporter();
                //gameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);

                TextureAssetExporter textureExporter = new TextureAssetExporter(this.path);
                gameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
                //gameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2DArray, textureExporter);

                gameStructure.Export($"{path}/exported/{filename}/", Texture2DAssetSelector);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file '{filename}'! you may ignore this.");
            }
        }

        private void btAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (thread != null && thread.IsAlive)
                {
                    MessageBox.Show($"Wait for old Thread to finish first");
                    return;
                }

                AllocConsole();
                Console.WriteLine($"Starting to download all files... this will take a while...");

                allFilesToDownload = this.manifestHelper.resources.ToList();

                thread = new Thread(new ThreadStart(DownloadMass));
                thread.Start();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while Downloading the files. Message: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null && thread.IsAlive)
            {
                MessageBox.Show($"Wait for old Thread to finish first");
                return;
            }

            AllocConsole();
            Console.WriteLine($"Starting to download and diff...");

            using (var client = new WebClient())
            {
                client.DownloadFile($"{txtUrlToCompareAssets.Text}manifest.data", $"{path}/Manifest/diff_manifest.data");
            }

            ManifestHelper manifestHelperDiff = new ManifestHelper();
            manifestHelperDiff.ReadFromFile($"{path}/Manifest/diff_manifest.data");

            var diff = manifestHelper.resources.Where(res => !manifestHelperDiff.resources.Contains(res)).ToList();

            Console.WriteLine($"New Entrys: {diff.Count}");


            foreach (var difference in diff)
            {
                Console.WriteLine(difference);
            }

            Console.WriteLine("Diff Completed. Now downloading...");

            allFilesToDownload = diff;

            thread = new Thread(new ThreadStart(DownloadMass));
            thread.Start();
        }
    }
}
