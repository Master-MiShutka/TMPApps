namespace TMP.ExcelOutput
{
    using System.Globalization;

    public static class ExcelOutputConfiguration
    {
        public static string DefaultDateTimeFormat { get; set; }

        public static string DefaultDateFormat { get; set; }

        public static string DefaultTimeFormat { get; set; }

        public static CultureInfo DefaultCulture { get; set; }
    }
}
