using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
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
    public sealed partial class DeveloperControlPage : Page
    {
        List<string> imageNames = new List<string>();

        public DeveloperControlPage()
        {
            this.InitializeComponent();
            LoadImageList();
        }

        public void LoadImageList()
        {
            DirectoryInfo directory = new DirectoryInfo(MainPage.storageFolder.Path);
            try
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    imageNames.Add(file.Name);
                }
            }
            catch
            {
                Debug.WriteLine("Directory is empty!");
            }
            RefreshLV();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            //Clear all files
            DirectoryInfo directory = new DirectoryInfo(MainPage.storageFolder.Path);
            try
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
            }
            catch
            {
                Debug.WriteLine("Nothing to delete!");
            }
            imageNames.Clear();
            RefreshLV();
        }

        public void RefreshLV()
        {
            lvLocalFiles.ItemsSource = null;
            lvLocalFiles.ItemsSource = imageNames;
        }

        public MessageDialog ShowMessageToUser(string message)
        {
            return new MessageDialog(message);
        }

        public async void UploadToFirebase(StorageFile file)
        {
            var stream = File.Open(file.Path, FileMode.Open);
            var task = new FirebaseStorage("ohwall-e865f.appspot.com")
                .Child("Media")
                .Child(file.DisplayName)
                .PutAsync(stream);
            task.Progress.ProgressChanged += (s, e) => ShowMessageToUser(Convert.ToString(e.Percentage));
            var downloadUrl = await task;
            var auth = "8qkIRcNDoQ5InGjxxhb7ax79c3WfJd0n2jyOgO70";
            var firebaseClient = new FirebaseClient(
              "https://ohwall-e865f.firebaseio.com/",
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth)
              });
            await firebaseClient
                .Child("media")
                .PostAsync(new Image(downloadUrl, "IOTCameraBooth-Backup"));
        }

        private async void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (StorageFile file in await MainPage.storageFolder.GetFilesAsync())
                {
                    UploadToFirebase(file);
                }
            }
            catch
            {
                ShowMessageToUser("Nothing to back up.");
            }
        }

        private void lvLocalFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(MainPage.storageFolder.Path);
            foreach (FileInfo file in directory.GetFiles())
            {
                if(file.Name == imageNames[lvLocalFiles.SelectedIndex])
                {
                    imgPhoto.Source = new BitmapImage(new Uri(file.FullName));
                }
            }
        }
    }
}
