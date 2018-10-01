using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCameraBooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public List<ImageSource> Props = new List<ImageSource>();
        public List<ImageSource> Stickers = new List<ImageSource>();

        public EditPage()
        {
            this.InitializeComponent();
            GetTakenImage();
            LoadProps();
        }

        public void LoadProps()
        {
            Props.Clear();
            DirectoryInfo directory = new DirectoryInfo("../AppX/Assets/Props");
            foreach (FileInfo file in directory.GetFiles())
            {
                Props.Add(new BitmapImage(new Uri(file.FullName)));
            }
        }

        public void LoadStickers()
        {
            Stickers.Clear();

        }

        public async void GetTakenImage()
        {
            StorageFile file = await MainPage.storageFolder.GetFileAsync(MainPage.globalObject.GetCurrentFile());
            imgViewer.Source = new BitmapImage(new Uri(file.Path));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainPage.globalObject.SetCurrentFile(null);
            this.Frame.Navigate(typeof(MainPage));
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UploadProgressPage));
        }

        private void imgViewer_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void imgViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                    // Set the image on the main page to the dropped image
                    imgViewer.Source = bitmapImage;
                }
            }
        }
    }
}
