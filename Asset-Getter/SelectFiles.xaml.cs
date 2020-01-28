using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
                var currentprefix = cbMulti.SelectedValue.ToString();

                var allFilesToDownload = this.manifestHelper.resources.Where(r => r.StartsWith(currentprefix)).ToList();

                foreach(string filename in allFilesToDownload)
                {
                    this.DownloadSingleFile(filename);
                }

                MessageBox.Show($"Completed. Look under {this.path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while Downloading the files. Message: {ex.Message}");
            }
        }

        private void DownloadSingleFile(string filename)
        {
            Directory.CreateDirectory($"{path}/tmp/{filename}");

            using (var client = new WebClient())
            {
                client.DownloadFile($"{assetUrl}{filename}.bundle", $"{path}/tmp/{filename}/{filename}.bundle");
            }
            List<string> pathes = new List<string>();
            pathes.Add($"{path}/tmp/{filename}/{filename}.bundle");
            var gameStructure = GameStructure.Load(pathes);
            EngineAssetExporter engineExporter = new EngineAssetExporter();
            gameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);

            TextureAssetExporter textureExporter = new TextureAssetExporter();
            gameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
            gameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Cubemap, textureExporter);
            gameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, textureExporter);

            gameStructure.Export($"{path}/exported/{filename}/", Texture2DAssetSelector);
        }
    }
}
