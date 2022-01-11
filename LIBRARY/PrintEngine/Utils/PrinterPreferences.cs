namespace TMP.PrintEngine.Utils
{
    using System;

    [Serializable]
    public class PrinterPreferences
    {
        public string PrinterName { get; set; }

        public bool IsMarkPageNumbers { get; set; }
    }
}
