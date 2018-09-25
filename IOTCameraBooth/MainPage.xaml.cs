using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Media;
using Windows.Graphics.Imaging;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.FileProperties;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IOTCameraBooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Screen Activity
        DisplayRequest displayRequest = new DisplayRequest();
        //Application State
        MediaCapture mediaCapture;
        bool isPreviewing;
        private StorageFolder captureFolder = null;
        private CameraRotationHelper _rotationHelper;
        //Storage
        public static StorageFolder storageFolder = null;
        //Global Variables
        int PID = 0;
        public static string currentImageFileName = null;

        public MainPage()
        {
            this.InitializeComponent();
            InitializeApp();
            StartPreviewAsync();
            Application.Current.Suspending += Application_Suspending;
        }


        public async void InitializeApp()
        {
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();
            mediaCapture.Failed += MediaCapture_Failed;
            //LocalFolder
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                storageFolder = await localFolder.CreateFolderAsync("Images", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                Debug.WriteLine("Storage folde exists");
            }
            storageFolder = await localFolder.GetFolderAsync("Images");
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            throw new NotImplementedException();
        }

        public MessageDialog ShowMessageToUser(string message)
        {
            return new MessageDialog(message);
        }

        private async Task StartPreviewAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
                displayRequest = new DisplayRequest();
                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
            }
            catch(UnauthorizedAccessException)
            {
                var messageDialog = ShowMessageToUser("No camera found.");

                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                messageDialog.Commands.Add(new UICommand(
                    "Close",
                    new UICommandInvokedHandler(this.CommandInvokedHandler)));

                // Set the command to be invoked when escape is pressed
                messageDialog.CancelCommandIndex = 0;

                // Show the message dialog
                await messageDialog.ShowAsync();

                return;
            }
            try
            {
                PreviewControl.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch(System.IO.FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }
        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                var messageDialog = ShowMessageToUser("The camera preview can't be displayed because another app has exclusive access");
                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                messageDialog.Commands.Add(new UICommand(
                    "Close",
                    new UICommandInvokedHandler(this.CommandInvokedHandler)));

                // Set the command to be invoked when escape is pressed
                messageDialog.CancelCommandIndex = 0;

                // Show the message dialog
                await messageDialog.ShowAsync();
            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }

        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }

        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
        }
        
        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        /// <summary>
        /// Takes a photo to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
     
        private async Task TakePhotoAsync()
        {
            var stream = new InMemoryRandomAccessStream();

            Debug.WriteLine("Taking photo...");
            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            try
            {
                var file = await captureFolder.CreateFileAsync("SimplePhoto.jpg", CreationCollisionOption.GenerateUniqueName);
                Debug.WriteLine("Photo taken! Saving to " + file.Path);

                var photoOrientation = CameraRotationHelper.ConvertSimpleOrientationToPhotoOrientation(_rotationHelper.GetCameraCaptureOrientation());

                await ReencodeAndSavePhotoAsync(stream, file, photoOrientation);
                Debug.WriteLine("Photo saved!");
            }
            catch (Exception ex)
            {
                // File I/O errors are reported as exceptions
                Debug.WriteLine("Exception when taking a photo: " + ex.ToString());
            }
        }

        private static async Task ReencodeAndSavePhotoAsync(IRandomAccessStream stream, StorageFile file, PhotoOrientation photoOrientation)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            throw new NotImplementedException();
        }

        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            CountDownTakePhoto();
        }

        public async void CountDownTakePhoto()
        {
            int cdt = 5;
            while (cdt > 0)
            {
                TextBlockTimer.Text = Convert.ToString(cdt);
                await Task.Delay(TimeSpan.FromSeconds(1));
                cdt -= 1;
            }
            TextBlockTimer.Text = "";
            TakePhotoAsyncV2();
            await Task.Delay(TimeSpan.FromSeconds(1));
            this.Frame.Navigate(typeof(EditPage));
        }

        private async void TakePhotoAsyncV2()
        {
            PID += 1;
            //string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            //string path = root + @"Images\";
            //StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            currentImageFileName = "OH2019Photo_" + PID + ".jpg";
            //StorageFile file = await folder.CreateFileAsync(currentImageFileName, CreationCollisionOption.GenerateUniqueName);
            StorageFile file = await storageFolder.CreateFileAsync(currentImageFileName, CreationCollisionOption.GenerateUniqueName);

            //Store in pictures folder
            //var myPictures = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            //StorageFile file = await myPictures.SaveFolder.CreateFileAsync(currentImageFileName, CreationCollisionOption.GenerateUniqueName);

            using (var captureStream = new InMemoryRandomAccessStream())
            {
                await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);

                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var decoder = await BitmapDecoder.CreateAsync(captureStream);
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(fileStream, decoder);

                    var properties = new BitmapPropertySet {
                        { "System.Photo.Orientation", new BitmapTypedValue(PhotoOrientation.Normal, PropertyType.UInt16) }
                    };
                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
            }
        }
    }
}