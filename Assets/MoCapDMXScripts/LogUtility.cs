using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoCapDMXScripts {
    public static class LogUtility {

        //Set true for performance logs in Logfile
        public static bool performanceTesting = false;
        public static bool performanceMethodsTesting = true;
        public static bool frameTimeTesting = false;

        public static System.IO.FileStream filestream = new System.IO.FileStream(
            @"C:\MoCapDMXUnity\MoCapDMX\LogFiles\MoCapDMX_LogFile_" + System.DateTime.Now.ToString(@"yyyy_MM_dd_hh-mm-ss") + ".txt",
            FileMode.CreateNew,
            FileAccess.Write);

        public static System.Diagnostics.Stopwatch TimeLog = new System.Diagnostics.Stopwatch();

        public static void StartLogTimer() {
            if (System.Diagnostics.Stopwatch.IsHighResolution) {
                Debug.Log("Stopwath is in Highresolution Mode");
            }
            TimeLog.Start();
        }

        private static float currentTime;

        public static double GetCurrentTime() {
            return TimeLog.Elapsed.TotalMilliseconds;
        }

        public static void LogToFile(string message) {
            if (CurrentMoCapFrame.Instance.frame != 0)
            {
                char[] msg = ("Frame " + CurrentMoCapFrame.Instance.frame + "||" + TimeLog.Elapsed.TotalMilliseconds.ToString() + " ms||" + message).ToCharArray();
                for (int i = 0; i < msg.Length; i++)
                {
                    filestream.WriteByte((byte)msg[i]);
                }
                char[] newline = System.Environment.NewLine.ToCharArray();
                for (int i = 0; i < newline.Length; i++)
                {
                    filestream.WriteByte((byte)newline[i]);
                }
            }
        }

        public static void SaveLogFile() {
            filestream.Close();
        }
    }
}
