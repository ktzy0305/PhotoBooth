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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IOTCameraBooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Initialisation
        MediaCapture mediaCapture;
        bool isPreviewing;
        DisplayRequest displayRequest = new DisplayRequest();


        public MainPage()
        {
            this.InitializeComponent();
            StartPreviewAsync();
            Application.Current.Suspending += Application_Suspending;
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

        private void CommandInvokedHandler(IUICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
