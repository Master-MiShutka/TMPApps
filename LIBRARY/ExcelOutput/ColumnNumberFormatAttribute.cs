using System;
using System.Globalization;

namespace TMP.ExcelOutput
{
    /// <summary>
    ///     An attribute that can be applied to a field or property which sets the column's Excel data type in an Excel export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnNumberFormatAttribute : Attribute
    {
        /// <summary>
        ///     Set a custom string format that should adhere to standard Excel formatting rules.
        /// </summary>
        /// <param name="numberFormat"></param>
        public ColumnNumberFormatAttribute(string numberFormat)
        {
            ExcelNumberFormat = ExcelNumberFormats.Custom;
            NumberFormat = numberFormat;
        }

        /// <summary>
        ///     Set the format using a predefined enumeration.
        /// </summary>
        /// <param name="excelNumberFormat"></param>
        public ColumnNumberFormatAttribute(ExcelNumberFormats excelNumberFormat)
        {
            var datePattern = ExcelOutputConfiguration.DefaultDateFormat;
            var timePattern = ExcelOutputConfiguration.DefaultTimeFormat;
            var currencySymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            var currencyDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
            var currencyGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;

            ExcelNumberFormat = excelNumberFormat;

            switch (excelNumberFormat)
            {
                case ExcelNumberFormats.General:
                    NumberFormat = "General";
                    break;
                case ExcelNumberFormats.Text:
                    NumberFormat = "@";
                    break;
                case ExcelNumberFormats.Number:
                    NumberFormat = "0";
                    break;
                case ExcelNumberFormats.Date:
                    NumberFormat = datePattern;
                    break;
                case ExcelNumberFormats.DateTime:
                    NumberFormat = datePattern + " " + timePattern;
                    break;
                case ExcelNumberFormats.Time:
                    NumberFormat = timePattern;
                    break;
                case ExcelNumberFormats.Currency:
                    NumberFormat = string.Concat("#", currencyGroupSeparator, "##0", currencyDecimalSeparator, "00 ", currencySymbol);
                    break;
                case ExcelNumberFormats.Accounting:
                    NumberFormat = string.Concat("_-* #", currencyGroupSeparator, "##0", currencyDecimalSeparator, "00 ", currencySymbol, "_-;-* #", currencyGroupSeparator, "##0", currencyDecimalSeparator, "00 ", currencySymbol, "_-;_-* \" - \"?? ", currencySymbol, "_-;_-@_-");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(excelNumberFormat), excelNumberFormat, null);
            }
        }

        public string NumberFormat { get; }

        public ExcelNumberFormats ExcelNumberFormat { get; }
    }
}
