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
                                                GetExceptionDetails(e),
                                                Priority.High);
            try
            {
                writer.WriteLine(messageToLog);
                writer.Flush();
            }
            catch { }
        }
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

        public static string GetExceptionDetails(Exception exp)
        {
            string message = string.Empty;
            try
            {
                // Write Message tree of inner exception into textual representation
                message = exp.Message;

                Exception innerEx = exp.InnerException;

                for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                {
                    string spaces = string.Empty;

                    for (int j = 0; j < i; j++)
                        spaces += "  ";

                    message += "\n" + spaces + "└─>" + innerEx.Message;
                }
                
                // Write complete stack trace info into details section
                message += "\nStack trace:\n" + exp.ToString();
            }
            catch
            {
            }

            return message;

        }


    }
}
