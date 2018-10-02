using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace IOTCameraBooth
{
    public class Prop
    {
        public int layer { get; set; }
        public ImageSource emoji { get; set; }

        public Prop(int l, ImageSource em)
        {
            layer = l;
            emoji = em;
        }
    }
}
