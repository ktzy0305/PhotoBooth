using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Global class to store global properties that will survive the lifetime of the app.
/// </summary>

namespace IOTCameraBooth
{
    public class SharedGlobals
    {
        public static int PID { get; set; }
        public static string CurrentFile { get; set; }
        public static string DownloadURL { get; set; }

        public SharedGlobals()
        {
            PID = 0;
        }

        public int GetPID()
        {
            return PID;
        }

        public void setPID(int id)
        {
            PID = id;
        }

        public void IncrementID()
        {
            PID += 1;
        }

        public string GetCurrentFile()
        {
            return CurrentFile;
        }

        public void SetCurrentFile(string filename)
        {
            CurrentFile = filename;
        }

        public string GetDownloadURL()
        {
            return DownloadURL;
        }

        public void SetDownloadURL(string url)
        {
            DownloadURL = url;
        }
    }
}
