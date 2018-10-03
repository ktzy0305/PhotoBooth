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
            imgViewer.Source = new BitmapImage(new Uri(file.Path));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainPage.globalObject.SetCurrentFile(null);
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void btnDone_Click(object sender, RoutedEventArgs e)
        {
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96);
            renderTarget.SetPixelBytes(new byte[(int)inkCanvas.ActualWidth * 4 * (int)inkCanvas.ActualHeight]);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                IReadOnlyList<InkStroke> inklist = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

                Debug.WriteLine("Ink_Strokes Count:  " + inklist.Count);
                ds.DrawInk(inklist);
            }
            var inkpixel = renderTarget.GetPixelBytes();
            WriteableBitmap bmp = new WriteableBitmap((int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight);
            Stream s = bmp.PixelBuffer.AsStream();
            s.Seek(0, SeekOrigin.Begin);
            s.Write(inkpixel, 0, (int)inkCanvas.ActualWidth * 4 * (int)inkCanvas.ActualHeight);

            //WriteableBitmap ink_wb = await ImageProcessing.ResizeByDecoderAsync(bmp, sourceImage.PixelWidth, sourceImage.PixelHeight, true);

            //WriteableBitmap combine_wb = await ImageProcessing.CombineAsync(sourceImage, ink_wb);

            this.Frame.Navigate(typeof(UploadProgressPage));
        }

        private void inkCanvas_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void inkCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Bitmap))
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
