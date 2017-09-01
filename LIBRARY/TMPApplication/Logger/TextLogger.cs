using System;
using System.Globalization;
using System.IO;

namespace TMP.Common.Logger
{
    /// <summary>
    /// Реализация интерфейса <см cref="ILoggerFacade"/> для ведения журнала <с помощью cref="TextWriter"/>.
    /// </summary>

    public class TextLogger : ILoggerFacade, IDisposable
    {
        private readonly TextWriter writer;
        public TextLogger() : this(Console.Out)
        {
        }
        public TextLogger(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            this.writer = writer;
        }
        public string TextLoggerPattern = "{0:HH:mm:ss:fff dd-MM-yyyy}\t{1}\t{3}\t{2}";

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Log(string message, Category category = Category.Info, Priority priority = Priority.None)
        {
            string messageToLog = String.Format(CultureInfo.InvariantCulture, TextLoggerPattern, DateTime.Now,
                                                category.ToString().ToUpper(CultureInfo.InvariantCulture), message, priority.ToString());
            try
            {
                writer.WriteLine(messageToLog);
                writer.Flush();
            }
            catch { }
        }
        public void Log(Exception e)
        {
            string messageToLog = String.Format(CultureInfo.InvariantCulture, TextLoggerPattern, DateTime.Now,
                                                Category.Exception,
                                                TMPApplication.TMPApp.GetExceptionDetails(e),
                                                Priority.High);
            try
            {
                writer.WriteLine(messageToLog);
                writer.Flush();
            }
            catch { }
        }

        public void Log(Exception e, string format = null)
        {
            if (String.IsNullOrEmpty(format) == false)
                Log(String.Format(format, TMPApplication.TMPApp.GetExceptionDetails(e)));
            else
                Log(e);
        }

        public void LogInfo(string message)
        {
            Log(message, Category.Info, Priority.Low);
        }

        public void LogWarning(string message)
        {
            Log(message, Category.Warn, Priority.Medium);
        }

        public void LogException(Exception exp)
        {
            Log(exp);
        }

        public void LogError(string message)
        {
            Log(message, Category.Exception, Priority.High);
        }

        public void Debug(string message)
        {
            Log(message, Category.Debug, Priority.None);
        }
    }
}
