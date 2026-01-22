using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TACM.UI.Utils
{
    public static class AppLogger
    {
        private static readonly string LogFilePath =
            Path.Combine(FileSystem.AppDataDirectory, "app-log.txt");

        public static void Log(string message)
        {
            try
            {
                var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}";
                File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
                Debug.WriteLine(logLine);
            }
            catch
            {
                // Avoid crashes — swallow errors
            }
        }

        public static string GetLogPath() => LogFilePath;
    }
}
