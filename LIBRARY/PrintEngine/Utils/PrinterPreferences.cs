using System;

namespace TMP.PrintEngine.Utils
{
    [Serializable]
    public class PrinterPreferences
    {
        public string PrinterName { get; set; }
        public bool IsMarkPageNumbers { get; set; }
    }
}
