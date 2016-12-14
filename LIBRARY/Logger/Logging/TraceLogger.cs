using System;
using System.Diagnostics;

namespace TMP.Common.Logger
{
    public class TraceLogger : ILoggerFacade
    {
        public void Log(Exception e)
        {
            Trace.TraceError(e.Message);
        }

        public void Log(string message, Category category, Priority priority)
        {
            if (category == Category.Exception)
            {
                Trace.TraceError(message);
            }
            else
            {
                Trace.TraceInformation(message);
            }
        }

    }
}
