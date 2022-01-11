namespace TMPApplication
{
    using System;
    using NLog;

    partial class TMPApp
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void ToDebug(Exception e)
        {
            int identLevel = System.Diagnostics.Debug.IndentLevel;
            do
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                e = e.InnerException;
                System.Diagnostics.Debug.Indent();
            }
            while (e != null);
            System.Diagnostics.Debug.IndentLevel = identLevel;
        }
    }
}
