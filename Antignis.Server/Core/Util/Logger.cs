using System;

namespace Antignis.Server.Core.Util
{
    internal class Logger
    {
        private static string appdataPath  = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string logDirectory = System.IO.Path.Combine(appdataPath, "Antignis");
        private static string debugLog     = System.IO.Path.Combine(logDirectory, "Antignis.debug.log");


        internal static void CheckLogDir()
        {
            if (!System.IO.Directory.Exists(logDirectory))
                System.IO.Directory.CreateDirectory(logDirectory);
        }

        /// <summary>
        /// Logic to write message to console
        /// </summary>
        /// <param name="message"></param>
        internal static void Log(string message)
        {
            string messageFormat = "[{0:dd-MM-yyyy HH:mm:ss}] - {1}";
            Console.WriteLine(messageFormat, DateTime.Now, message);
        }

        /// <summary>
        /// Logic to write verbose message to console
        /// </summary>
        /// <param name="message"></param>
        internal static void LogVerbose(string message)
        {
            if (Program.Verbose)
            {
                string messageFormat = "[VERBOSE] [{0:dd-MM-yyyy HH:mm:ss}] - {1}";
                Console.WriteLine(messageFormat, DateTime.Now, message);
            }
        }

        /// <summary>
        /// Logic to write debug message to console
        /// </summary>
        /// <param name="message"></param>
        internal static void LogDebug(string message)
        {
            if (Program.Debug)
            {
                string messageFormat = "[DEBUG] [{0:dd-MM-yyyy HH:mm:ss}] - {1}";
                Console.WriteLine(messageFormat, DateTime.Now, message);

                System.IO.File.AppendAllText(debugLog, message);
            }
        }
    }
}
