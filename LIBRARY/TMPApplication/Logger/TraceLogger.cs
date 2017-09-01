using System;
using System.Diagnostics;

namespace TMP.Common.Logger
{
    public class TraceLogger : ILoggerFacade
    {
        public void Debug(string message)
        {
            Log(message, Category.Debug, Priority.None);
        }

        public void Log(Exception e)
        {
            Trace.TraceError(e.Message);
        }

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    Debug(message);
                    break;
                case Category.Exception:
                    LogError(message);
                    break;
                case Category.Info:
                    LogInfo(message);
                    break;
                case Category.Warn:
                    LogWarning(message);
                    break;
                default:
                    throw new NotImplementedException(category.ToString());
            }
        }

        public void Log(Exception e, string format = null)
        {
            if (String.IsNullOrEmpty(format) == false)
                Trace.TraceError(String.Format(format, TMPApplication.TMPApp.GetExceptionDetails(e)));
            Trace.TraceError(TMPApplication.TMPApp.GetExceptionDetails(e));
        }

        public void LogException(Exception exp)
        {
            Log(exp);
        }

        public void LogError(string message)
        {
            Trace.TraceError(message);
        }

        public void LogInfo(string message)
        {
            Trace.TraceInformation(message);
        }

        public void LogWarning(string message)
        {
            Trace.TraceWarning(message);
        }
    }
}
