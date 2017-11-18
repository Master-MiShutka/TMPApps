using System;

namespace TMPApplication.Logger
{
    using TMP.Common.Logger;
    public class ConsoleLogger : ILoggerFacade
    {
        private static string _pattern = "{0:HH:mm:ss:fff dd-MM-yyyy}\t{1}\t{2}";

        public void Log(string message, Category category = Category.Info, Priority priority = Priority.None)
        {
            WriteToConsole(message, category, priority);
        }

        public void Log(Exception e)
        {
            WriteToConsole(TMPApplication.TMPApp.GetExceptionDetails(e), Category.Exception, Priority.High);
        }

        public void Log(Exception e, string format = null)
        {
            if (String.IsNullOrEmpty(format) == false)
                WriteToConsole(String.Format(format, TMPApplication.TMPApp.GetExceptionDetails(e)), Category.Exception, Priority.High);
            else
                WriteToConsole(e.Message, Category.Exception, Priority.High);
        }

        public void LogException(Exception exp)
        {
            Log(exp);
        }

        public void LogError(string message)
        {
            WriteToConsole(message, Category.Exception, Priority.High);
        }

        public void LogInfo(string message)
        {
            WriteToConsole(message, Category.Info);
        }

        public void LogWarning(string message)
        {
            WriteToConsole(message, Category.Warn, Priority.Medium);
        }

        public void Debug(string message)
        {
            LogInfo(message);
        }

        private void WriteToConsole(string message, Category category, Priority priority = Priority.None)
        {
            ConsoleColor _oldForegroud = Console.ForegroundColor;
            ConsoleColor _oldBackgroud = Console.BackgroundColor;
            switch (category)
            {
                case Category.Debug:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case Category.Exception:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Category.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case Category.Warn:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    throw new NotImplementedException(category.ToString());
            }
            try
            {
                string messageToLog = String.Format(System.Globalization.CultureInfo.InvariantCulture, _pattern, DateTime.Now, category, message);
                Console.WriteLine(messageToLog);
            }
            finally
            {
                Console.ForegroundColor = _oldForegroud;
                Console.BackgroundColor = _oldBackgroud;
            }
        }
    }
}
