using System;
using System.Globalization;
using System.Windows.Data;

namespace TMP.UI.Controls.WPF.Converters
{
    public class UnderLineTextToSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "<не указано>";
            string s = value.ToString();
            if (String.IsNullOrWhiteSpace(s))
                return "<пусто>";
            return s.Replace("_", " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Replace(" ", "_");
        }
    }
}
