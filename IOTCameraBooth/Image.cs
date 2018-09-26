using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;

namespace IOTCameraBooth
{
    public class Image
    {
        public string image { get; set; }
        public string source { get; set; }

        public Image(string url, string s)
        {
            image = url;
            source = s;
        }
    }
}
