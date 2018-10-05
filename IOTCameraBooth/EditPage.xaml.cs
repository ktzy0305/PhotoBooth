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
        }

        public void LoadProps()
        {
            Props.Clear();
            DirectoryInfo directory = new DirectoryInfo("../AppX/Assets/Props");
            foreach (FileInfo file in directory.GetFiles())
            {
                Props.Add(new BitmapImage(new Uri(file.FullName)));
            }
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            Windows.UI.Core.CoreInputDeviceTypes.Pen |
            Windows.UI.Core.CoreInputDeviceTypes.Touch;
        }

        public void LoadStickers()
        {
            Stickers.Clear();
        }

        public async void GetTakenImage()
        {
            //Get original captured image file
            StorageFile file = await MainPage.storageFolder.GetFileAsync(MainPage.globalObject.GetCurrentFile());
            //Open the file of the original image and store it as a filestream.
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            //Create an image decoder with the filestream.
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            //Create a new writeableBitmap with the original captured image width and height.
            WriteableBitmap writeableBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            //Set the source of the writable bitmap as the file stream of the opened original picture file.
            writeableBitmap.SetSource(stream);
            
            //Find the sticker file in the local directory.
            FileInfo stickerFile = new FileInfo("../AppX/Assets/Stickers/StickerPlaceHolder.png");
            //Get the sticker file from the directory.
            StorageFile sticker = await StorageFile.GetFileFromPathAsync(stickerFile.FullName);
            //Open the sticker file and store it as a filestream.
            IRandomAccessStream stickerstream = await sticker.OpenAsync(FileAccessMode.Read);
            //Create an image decoder with the filestream.
            BitmapDecoder stickerDecoder = await BitmapDecoder.CreateAsync(stickerstream);
            //Create a new writeable bitmap with the stickers width and height
            WriteableBitmap stickerWriteableBitmap = new WriteableBitmap((int)stickerDecoder.PixelWidth, (int)stickerDecoder.PixelHeight);
            //Set the source of the sticker's writable bitmap as the file stream of the sticker.
            stickerWriteableBitmap.SetSource(stickerstream);
            stickerWriteableBitmap.Resize(25, 3, WriteableBitmapExtensions.Interpolation.Bilinear);
            //Copy the sticker writable bitmap to the image writeable bitmap
            writeableBitmap.Blit(new Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight), stickerWriteableBitmap, new Rect(0, 0, stickerWriteableBitmap.PixelWidth, stickerWriteableBitmap.PixelHeight));
            //Set the imgViewer source as the edited image.
            imgViewer.Source = writeableBitmap;
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
