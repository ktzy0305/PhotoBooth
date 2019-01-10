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
using Windows.System;
using Windows.UI.Text;

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
        //Storage
        public static StorageFolder storageFolder;
        //Global Variables
        public static SharedGlobals globalObject;

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
                Debug.WriteLine("Storage folder exists");
            }
            if (storageFolder == null)
            {
                storageFolder = await localFolder.GetFolderAsync("Images");
            }
            if (globalObject == null)
            {
                globalObject = new SharedGlobals();
            }
            DirectoryInfo directory = new DirectoryInfo(MainPage.storageFolder.Path);
            globalObject.setPID(directory.GetFiles().Count() + 1);
            TextBlockInstructions.Text = "Smile to the camera!";
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
                // get available resolutions (Not recommended unless your current camera stream is not at its maximum resolution)
                //var resolutions = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).ToList();
                // set used resolution (Set a breakpoint here and read the resolution of each media stream then set to the one you want below)
                //await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, resolutions[1]);
            }
            catch (UnauthorizedAccessException)
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
            catch (System.IO.FileLoadException)
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
                messageDialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(this.CommandInvokedHandler)));

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

        private void CommandInvokedHandler(IUICommand command)
        {
            throw new NotImplementedException();
        }

        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            CameraButton.IsEnabled = false;
            CameraButton.Opacity = 0;
            CountDownTakePhoto();
        }

        public async void CountDownTakePhoto()
        {
            TextBlockInstructions.Text = "";
            int cdt = 5;
            while (cdt > 0)
            {
                TextBlockTimer.Text = Convert.ToString(cdt);
                await Task.Delay(TimeSpan.FromSeconds(1));
                cdt -= 1;
            }
            TakePhotoAsyncV2();
            TextBlockTimer.Text = "";
            await Task.Delay(TimeSpan.FromSeconds(1));
            this.Frame.Navigate(typeof(EditPage));
        }

        private async void TakePhotoAsyncV2()
        {
            globalObject.SetCurrentFile(globalObject.GetPID().ToString("0000") + "-OH2019OriginalPhoto.jpg");
            StorageFile file = await storageFolder.CreateFileAsync(globalObject.GetCurrentFile(), CreationCollisionOption.GenerateUniqueName);

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

        //Developer Controls
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && Window.Current.CoreWindow.GetKeyState(VirtualKey.F12).HasFlag(CoreVirtualKeyStates.Down))
            {
                this.Frame.Navigate(typeof(DeveloperControlPage));
            }
        }
    }
}