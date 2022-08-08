namespace TMPApplication
{
    using NLog;
    using System;

    partial class TMPApp
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void ToDebug(Exception e)
        {
            System.Diagnostics.Debug.WriteLine(GetExceptionDetails(e));
        }
    }
}
