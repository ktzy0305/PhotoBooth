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
using System.Threading.Tasks;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCameraBooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public List<Prop> Props = new List<Prop>();
        public List<ImageSource> Stickers = new List<ImageSource>();
        public List<ImageSource> Edits = new List<ImageSource>();

        //Storage for edited photos
        public static StorageFolder editsFolder;

        WriteableBitmap writeableBitmap;

        public EditPage()
        {
            this.InitializeComponent();
            InitEditPage();
            GetTakenImage();
            LoadProps();
        }

        public async void InitEditPage()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                editsFolder = await localFolder.CreateFolderAsync("EditedImages", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                Debug.WriteLine("Edited images folder exists");
            }
            if (editsFolder == null)
            {
                editsFolder = await localFolder.GetFolderAsync("EditedImages");
            }
        }

        public void LoadProps()
        {
            Props = SharedGlobals.Props;
            /*
            Props.Clear();
            int i = 0;
            DirectoryInfo directory = new DirectoryInfo("../AppX/Assets/Props");
            foreach (FileInfo file in directory.GetFiles())
            {
                //Props.Add(new Prop(i, new BitmapImage(new Uri(file.FullName))));
                Props.Add(new Prop(i, file.FullName));
                i += 1;
            }*/
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
            /*WriteableBitmap*/
            writeableBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            //Set the source of the writable bitmap as the file stream of the opened original picture file.
            writeableBitmap.SetSource(stream);

            //Find the sticker file in the local directory.
            FileInfo stickerFile = new FileInfo("../AppX/Assets/Stickers/PlaceHolder.png");
            //Get the sticker file from the directory.
            StorageFile sticker = await StorageFile.GetFileFromPathAsync(stickerFile.FullName);
            //Open the sticker file and store it as a filestream.
            IRandomAccessStream stickerstream = await sticker.OpenAsync(FileAccessMode.Read);
            //Create an image decoder with the filestream.
            BitmapDecoder stickerDecoder = await BitmapDecoder.CreateAsync(stickerstream);
            //Create a new writeable bitmap with the stickers width and height.
            WriteableBitmap stickerWriteableBitmap = new WriteableBitmap((int)stickerDecoder.PixelWidth, (int)stickerDecoder.PixelHeight);
            //Set the source of the sticker's writable bitmap as the file stream of the sticker.
            stickerWriteableBitmap.SetSource(stickerstream);
            stickerWriteableBitmap.Resize(25, 3, WriteableBitmapExtensions.Interpolation.Bilinear);
            //Copy the sticker writable bitmap to the image writeable bitmap
            writeableBitmap.Blit(new Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight), stickerWriteableBitmap, new Rect(0, 0, stickerWriteableBitmap.PixelWidth, stickerWriteableBitmap.PixelHeight));


            //Convert edited writable bitmap to a jpeg image file.
            //StorageFile editedFile = await WriteableBitmapToStorageFile(writeableBitmap, FileFormat.Jpeg);

            //Set the imgViewer source as the edited image..
            imgViewer.Stretch = Stretch.Uniform;
            imgViewer.Source = writeableBitmap;
        }

        private async Task<StorageFile> WriteableBitmapToStorageFile(WriteableBitmap WB, FileFormat fileFormat)
        {
            string FileName = MainPage.globalObject.GetPID().ToString("0000") + "-OH2019Photo.";
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            switch (fileFormat)
            {
                case FileFormat.Jpeg:
                    FileName += "jpeg";
                    BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
                    break;

                case FileFormat.Png:
                    FileName += "png";
                    BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
                    break;

                case FileFormat.Bmp:
                    FileName += "bmp";
                    BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
                    break;

                case FileFormat.Tiff:
                    FileName += "tiff";
                    BitmapEncoderGuid = BitmapEncoder.TiffEncoderId;
                    break;

                case FileFormat.Gif:
                    FileName += "gif";
                    BitmapEncoderGuid = BitmapEncoder.GifEncoderId;
                    break;
            }

            //Set current file of global objects to the edited file.
            MainPage.globalObject.SetCurrentFile(FileName);

            var file = await editsFolder.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)WB.PixelWidth,
                                    (uint)WB.PixelHeight,
                                    96.0,
                                    96.0,
                                    pixels);
                await encoder.FlushAsync();
            }
            return file;
        }

        private enum FileFormat
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
            Gif
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainPage.globalObject.SetCurrentFile(null);
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void btnDone_Click(object sender, RoutedEventArgs e)
        {
            //var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            //canvas on display has different scaling compare to screen scaling
            //photo is 3264x1836

            //TODO: fullscreen mode affected the aspect ratio. verify aspect ratio on machine 
            var scaleFactor = writeableBitmap.PixelWidth / canvas.ActualWidth;
            var marginTop = (canvas.ActualHeight - writeableBitmap.PixelHeight / scaleFactor) / 2.0;
            //var scaleFactorH = 1836 / canvas.ActualHeight;
            foreach (Windows.UI.Xaml.Controls.Image i in canvas.Children)
            {
                double x = Canvas.GetLeft(i) * scaleFactor;
                double y = (Canvas.GetTop(i) - marginTop) * scaleFactor;
                double width = 110 * scaleFactor;
                double height = 110 * scaleFactor;
                string name = i.Name;

                WriteableBitmap wb = Props[Convert.ToInt32(name)].sticker.Resize((int)width, (int)height, WriteableBitmapExtensions.Interpolation.Bilinear);
                writeableBitmap.Blit(new Rect(x, y, width, height), wb, new Rect(0, 0, width, height));
            }

            StorageFile editedFile = await WriteableBitmapToStorageFile(writeableBitmap, FileFormat.Jpeg);

            this.Frame.Navigate(typeof(UploadProgressPage));
        }

        private void canvas_DragStarting(UIElement sender, DragStartingEventArgs e)
        {
            e.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private void canvas_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            //e.DragUIOverride.IsContentVisible = true;
        }

        private async void canvas_Drop(object sender, DragEventArgs e)
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

        private void lvProps_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var id = e.Items.Cast<Prop>().First().id.ToString(); //e.Items.Cast<Prop>().Select(i => i.id).ToString();
            e.Data.SetText(id);
            e.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private void dropEmojiList_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }

        private async void dropEmojiList_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var id = await e.DataView.GetTextAsync(); //should hav only 1 id
                //Edits.Add(Props[Convert.ToInt32(id)].emoji);
                Point p = e.GetPosition(canvas);

                //writeableBitmap.Blit(new Rect(0, 0, 512, 512), Props[Convert.ToInt32(id)].sticker, new Rect(0, 0, 512, 512));
                //imgViewer.Source = writeableBitmap;
                Windows.UI.Xaml.Controls.Image i = new Windows.UI.Xaml.Controls.Image();
                i.Name = Convert.ToInt32(id).ToString();
                i.Source = Props[Convert.ToInt32(id)].emoji;
                i.Height = 110;
                i.Width = 110;
                i.RenderTransform = new CompositeTransform();
                i.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                i.ManipulationStarting += Canvas_ManipulationStarting;
                i.ManipulationDelta += canvas_CalculateDelta;
                i.ManipulationCompleted += Canvas_ManipulationCompleted;
                Canvas.SetLeft(i, p.X - 55);
                Canvas.SetTop(i, p.Y - 55);
                canvas.Children.Add(i);

            }
        }

        private void canvas_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void canvas_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void Canvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }

        private void canvas_CalculateDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //imgTest.TranslateX += e.Delta.Translation.X;
            //imgTest.TranslateY += e.Delta.Translation.Y;
            var image = (Windows.UI.Xaml.Controls.Image)sender;
            var transform = (CompositeTransform)image.RenderTransform;
            //transform.TranslateX += e.Delta.Translation.X;
            //transform.TranslateY += e.Delta.Translation.Y;
            Canvas.SetLeft(image, Canvas.GetLeft(image) + e.Delta.Translation.X);
            Canvas.SetTop(image, Canvas.GetTop(image) + e.Delta.Translation.Y);

        }

        private void Canvas_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {

        }
    }
}
