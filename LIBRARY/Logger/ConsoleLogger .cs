using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Common.Logger
{
    public class ConsoleLogger : ILogger
    {
        public ServerLogLevel LogLevel { get; set; }

        public ConsoleLogger(ServerLogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        public void Info(string text, params object[] args)
        {
            if (LogLevel < ServerLogLevel.Info)
                return;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Conlog("(I) ", text, args);
        }

        public void Warn(string text, params object[] args)
        {
            if (LogLevel < ServerLogLevel.Warn)
                return;
            Console.ForegroundColor = ConsoleColor.Green;
            Conlog("(W) ", text, args);
        }

        public void Error(Exception ex)
        {
            if (LogLevel < ServerLogLevel.Error)
                return;
            Console.ForegroundColor = ConsoleColor.Red;
            Conlog("(E) ", ex.ToString());
        }

        public void Error(string text, params object[] args)
        {
            if (LogLevel < ServerLogLevel.Error)
                return;
            Console.ForegroundColor = ConsoleColor.Red;
            Conlog("(E) ", text, args);
        }

        public void Error(Exception ex, string text, params object[] args)
        {
            if (LogLevel < ServerLogLevel.Error)
                return;
            Console.ForegroundColor = ConsoleColor.Red;
            Conlog("(E) ", text, args);
        }

        public void Debug(string text, params object[] args)
        {
            if (LogLevel < ServerLogLevel.Debug)
                return;
            Conlog("(D) ", text, args);
        }

        public static void Conlog(string prefix, string text, params object[] args)
        {
            //            If you want to add unique thread identifier
            //            int threadId = Thread.CurrentThread.ManagedThreadId;
            //            Console.Write("[{0:D4}] [{1}] ", threadId, DateTime.Now.ToString("HH:mm:ss.ffff"));
            Console.Write(DateTime.Now.ToString("HH:mm:ss.ffff"));
            Console.Write(prefix);
            Console.WriteLine(text, args);
            Console.ResetColor();
        }
    }
}