using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}
