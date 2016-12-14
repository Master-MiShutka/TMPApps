using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Converters
{
    public class NewLineRemoveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return value.ToString().Replace("\n", "");
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}