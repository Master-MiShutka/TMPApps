using System.Globalization;

namespace TMP.ExcelOutput
{
    public static class ExcelOutputConfiguration
    {
        public static string DefaultDateTimeFormat { get; set; }
        public static string DefaultDateFormat { get; set; }
        public static string DefaultTimeFormat { get; set; }
        public static CultureInfo DefaultCulture { get; set; }
    }
}
