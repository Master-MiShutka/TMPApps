using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.UI.Controls.WPF.Converters
{
    [System.Windows.Data.ValueConversion(typeof(int), typeof(int))]
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            int pos = -1;
            if (int.TryParse(value.ToString(), out pos))
                return ++pos;
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
