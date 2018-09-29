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
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using System.Diagnostics;
using Firebase.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCameraBooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadProgressPage : Page
    {
        public UploadProgressPage()
        {
            this.InitializeComponent();
            GetUploadingImage();
        }

        public async void GetUploadingImage()
        {
            StorageFile file = await MainPage.storageFolder.GetFileAsync(MainPage.globalObject.GetCurrentFile());
            imgUploadingPhoto.Source = new BitmapImage(new Uri(file.Path));
            UploadToFirebase(file);
        }

        public async void UploadToFirebase(StorageFile file)
        {
            var stream = File.Open(file.Path, FileMode.Open);
            var task = new FirebaseStorage("ohwall-e865f.appspot.com")
                .Child("Media")
                .Child(file.DisplayName)
                .PutAsync(stream);
            task.Progress.ProgressChanged += (s, e) => progressBar.Value = e.Percentage;
            var downloadUrl = await task;
            MainPage.globalObject.SetDownloadURL(downloadUrl);
            var auth = "8qkIRcNDoQ5InGjxxhb7ax79c3WfJd0n2jyOgO70";
            var firebaseClient = new FirebaseClient(
              "https://ohwall-e865f.firebaseio.com/",
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth)
              });
            await firebaseClient
                .Child("media")
                .PostAsync(new Image(MainPage.globalObject.GetDownloadURL(),"IOTCameraBooth"));
            if (progressBar.Value == 100)
            {
                this.Frame.Navigate(typeof(UploadCompletePage));
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
        }
    }
}
