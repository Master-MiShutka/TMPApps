using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.UI.Controls.WPF.Converters
{
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null || String.IsNullOrWhiteSpace(value.ToString())) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}