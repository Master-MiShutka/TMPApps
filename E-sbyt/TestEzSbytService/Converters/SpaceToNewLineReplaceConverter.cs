using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Converters
{
    public class SpaceToNewLineReplaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                //.Replace(" ", "\n")
                return value.ToString().Replace("_", " ");
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}