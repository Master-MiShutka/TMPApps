using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Converters
{
    public class RequestFuncTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            EzSbyt.EzSbytRequestFunctionType func;
            if (Enum.TryParse<EzSbyt.EzSbytRequestFunctionType>(value.ToString(), out func) == false)
                return DependencyProperty.UnsetValue;
            return func;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(String))
                return value.ToString();
            else
                return DependencyProperty.UnsetValue;
        }
    }
}