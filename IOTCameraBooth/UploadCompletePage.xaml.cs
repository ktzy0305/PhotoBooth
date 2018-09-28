using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.Storage.Streams;
using ZXing.Net.Mobile;
using ZXing.Mobile;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCameraBooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadCompletePage : Page
    {
        public UploadCompletePage()
        {
            this.InitializeComponent();
            GetUploadedImage();
        }

        public async void GetUploadedImage()
        {
            StorageFile file = await MainPage.storageFolder.GetFileAsync(MainPage.globalObject.GetCurrentFile());
            imgUploadedPhoto.Source = new BitmapImage(new Uri(file.Path));
            var write = new BarcodeWriter();
            write.Format = ZXing.BarcodeFormat.QR_CODE;
            imgQRCode.Source = write.Write(MainPage.globalObject.GetDownloadURL());
        }

        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPage.globalObject.SetCurrentFile(null);
            MainPage.globalObject.SetDownloadURL(null);
            this.Frame.Navigate(typeof(StartScreen));            
        }
    }
}
