using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace Asset_Getter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path = Directory.GetCurrentDirectory();

        public MainWindow()
        {
            InitializeComponent();
            tbPath.Text = path;
        }

        private void btGetListOfAssets_Clicked(object sender, RoutedEventArgs e)
        {
            //https://eaassets-a.akamaihd.net/assetssw.capitalgames.com/PROD/18312/Android/ETC/

            Directory.CreateDirectory($"{path}/Manifest");

            using (var client = new WebClient())
            {
                client.DownloadFile($"{txtUrlToAssets.Text}manifest.data", $"{path}/Manifest/manifest.data");
            }

            ManifestHelper manifestHelper = new ManifestHelper();
            manifestHelper.ReadFromFile($"{path}/Manifest/manifest.data");

            SelectFiles sf = new SelectFiles(manifestHelper, path, txtUrlToAssets.Text);
            this.Close();
        }

        private void BtSetPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog();
            WinForms.DialogResult result = dialog.ShowDialog();

            if(result == WinForms.DialogResult.OK)
            {
                this.path = dialog.SelectedPath;
            }

            tbPath.Text = path;
        }
    }
}
