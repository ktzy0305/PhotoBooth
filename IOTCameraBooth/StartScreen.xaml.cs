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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCameraBooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartScreen : Page
    {
        public StartScreen()
        {
            this.InitializeComponent();
            BlinkingText();
            LoadProps();
        }

        public void LoadProps()
        {
            SharedGlobals.Props.Clear();
            int i = 0;
            DirectoryInfo directory = new DirectoryInfo("../AppX/Assets/Props");
            foreach (FileInfo file in directory.GetFiles())
            {
                //Props.Add(new Prop(i, new BitmapImage(new Uri(file.FullName))));
                SharedGlobals.Props.Add(new Prop(i, file.FullName));
                i += 1;
            }
        }

        public void BlinkingText()
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(2.0));
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                BeginTime = TimeSpan.FromSeconds(1),
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };
            Storyboard.SetTarget(opacityAnimation, txtTouchAnywhere);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            storyboard.Children.Add(opacityAnimation);
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.AutoReverse = true;
            storyboard.Begin();
        }

        private void screen_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
