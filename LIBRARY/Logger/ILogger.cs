using System;

namespace TMP.Common.Logger
{
    public interface ILogger
    {
        ServerLogLevel LogLevel { get; }

        void Debug(string text, params object[] args);

        void Info(string text, params object[] args);

        void Warn(string text, params object[] args);

        void Error(string text, params object[] args);

        void Error(Exception ex);

        void Error(Exception ex, string text, params object[] args);
    }
}