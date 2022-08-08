namespace TMP.UI.WPF.Controls.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class UnderLineTextToSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "<не указано>";
            }

            string s = value.ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return "<пусто>";
            }

            return s.Replace("_", " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Replace(" ", "_");
        }
    }
}
