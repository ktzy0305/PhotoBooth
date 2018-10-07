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
        public int id { get; set; }
        public ImageSource emoji { get; set; }

        public Prop(int i, ImageSource em)
        {
            id = i;
            emoji = em;
        }
    }
}
