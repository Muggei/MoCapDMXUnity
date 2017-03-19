using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoCapDMXScripts {
    public static class LogUtility {

        public static System.IO.FileStream filestream = new System.IO.FileStream(
            @"C:\MoCapDMXUnity\MoCapDMX\LogFiles\LogFile_" + System.DateTime.Now.ToString(@"yyyy_MM_dd_hh-mm-ss") + ".txt",
            FileMode.CreateNew,
            FileAccess.Write);

        public static System.Diagnostics.Stopwatch TimeLog = new System.Diagnostics.Stopwatch();

        public static void StartLogTimer() {
            TimeLog.Start();
        }

        private static float currentTime;

        public static void LogToFile(string message) {
            char[] msg = ("Frame " + CurrentMoCapFrame.Instance.frame + "||" + TimeLog.ElapsedMilliseconds.ToString() + "||" + message).ToCharArray();
            for (int i = 0; i < msg.Length; i++) {
                filestream.WriteByte((byte)msg[i]);
            }
            char[] newline = System.Environment.NewLine.ToCharArray();
            for (int i = 0; i < newline.Length; i++)
            {
                filestream.WriteByte((byte)newline[i]);
            }
        }

        public static void SaveLogFile() {
            filestream.Close();
        }
    }
}
