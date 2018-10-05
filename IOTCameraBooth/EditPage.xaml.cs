using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;


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
        public List<ImageSource> Edits = new List<ImageSource>();

        public EditPage()
        {
            this.InitializeComponent();
            GetTakenImage();
            LoadProps();
            inkCanvas.InkPresenter.InputDeviceTypes  = Windows.UI.Core.CoreInputDeviceTypes.Mouse | 
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
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
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            WriteableBitmap image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            image.SetSource(stream);
            FileInfo stickerFile = new FileInfo("../AppX/Assets/Stickers/StickerPlaceHolder.png");
            StorageFile sticker = await StorageFile.GetFileFromPathAsync(stickerFile.FullName);
            IRandomAccessStream stream2 = await sticker.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder2 = await BitmapDecoder.CreateAsync(stream2);
            WriteableBitmap image2 = new WriteableBitmap((int)decoder2.PixelWidth, (int)decoder2.PixelHeight);
            image.Blit(new Rect(0, 0, image.PixelWidth, image.PixelHeight), image2, new Rect(0, 0, image2.PixelWidth, image2.PixelHeight));
            //imgViewer.Source = new BitmapImage(new Uri(file.Path));
            imgViewer.Source = image;
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

        private void inkCanvas_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void inkCanvas_Drop(object sender, DragEventArgs e)
        {
            //DataPackageView d = e.DataView;
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var storageItems = await e.DataView.GetStorageItemsAsync();
                foreach (StorageFile storageItem in storageItems)
                {
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(await storageItem.OpenReadAsync());
                    Edits.Add(bitmapImage);
                }
            }
        }
    }
}
