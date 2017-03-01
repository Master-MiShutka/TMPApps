using System;
using System.IO;
using System.Windows;
using TMP.Common.Logger;

namespace TMPApplication
{
    partial class TMPApp
    {
        #region Logging

        public static readonly string LOG_FILE_NAME =
            System.IO.Path.GetFileNameWithoutExtension(Application.ResourceAssembly.Location) + ".log";

        ILoggerFacade IAppWithLogger.Logger { get { return TMPApp.Logger; } }

        public static ILoggerFacade Logger { get; } =
            new TextLogger(new StreamWriter(LOG_FILE_NAME, false, System.Text.Encoding.UTF8)
            { AutoFlush = true });

        public static void LogException(Exception e)
        {
            Logger.Log(e);
        }
        public static void Log(Exception e, string format = null)
        {
            Logger.Log(e, format);
        }
        public static void Log(string message)
        {
            Logger.Log(message);
        }
        public static void LogInfo(string message)
        {
            Logger.LogInfo(message);
        }
        public static void LogError(string message)
        {
            Logger.LogError(message);
        }
        public static void LogWarning(string message)
        {
            Logger.LogWarning(message);
        }
        #endregion
    }
}
