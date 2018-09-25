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
        public static string currentFile { get; set; }

        public SharedGlobals()
        {
            PID = 0;
        }

        public int getPID()
        {
            return PID;
        }

        public void IncrementID()
        {
            PID += 1;
        }

        public string getCurrentFile()
        {
            return currentFile;
        }

        public void setCurrentFile(string filename)
        {
            currentFile = filename;
        }
    }
}
