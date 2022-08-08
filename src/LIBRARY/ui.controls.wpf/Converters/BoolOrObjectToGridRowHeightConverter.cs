using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.UI.WPF.Controls.Converters
{
    [ValueConversion(typeof(bool), typeof(GridLength))]
    [ValueConversion(typeof(object), typeof(GridLength))]
    public class BoolOrObjectToGridRowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new GridLength(0);
            }
            else
            {
                int size = 1;
                if (parameter != null && parameter is string str)
                {
                    int.TryParse(str, out size);
                }

                if (value is bool b)
                {
                    return (b == true) ? new GridLength(size, GridUnitType.Star) : new GridLength(0);
                }

                return new GridLength(size, GridUnitType.Star);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Don't need any convert back
            return null;
        }
    }
}