using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace IOTCameraBooth
{
    public class Prop
    {
        public int id { get; set; }
        public ImageSource emoji { get; set; }
        public WriteableBitmap sticker { get; set; }

        public Prop(int i, ImageSource em)
        {
            id = i;
            emoji = em;
        }

        public Prop(int i, string src)
        {
            id = i;
            emoji = new BitmapImage(new Uri(src));

            LoadWriteable(src);
        }

        private async void LoadWriteable(string src)
        {
            //Get the sticker file from the directory.
            StorageFile sf = await StorageFile.GetFileFromPathAsync(src);
            //Open the sticker file and store it as a filestream.
            IRandomAccessStream ras = await sf.OpenAsync(FileAccessMode.Read);
            //Create an image decoder with the filestream.
            BitmapDecoder sd = await BitmapDecoder.CreateAsync(ras);
            //Create a new writeable bitmap with the stickers width and height.
            WriteableBitmap stickerWriteableBitmap = new WriteableBitmap((int)sd.PixelWidth, (int)sd.PixelHeight);
            //Set the source of the sticker's writable bitmap as the file stream of the sticker.
            stickerWriteableBitmap.SetSource(ras);

            sticker = stickerWriteableBitmap; // stickerWriteableBitmap.Resize(110, 110, WriteableBitmapExtensions.Interpolation.Bilinear);
        }
    }
}
