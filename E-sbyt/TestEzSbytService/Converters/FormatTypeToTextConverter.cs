using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Converters
{
    public class FormatTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            if (value is ViewModel.Funcs.FuncParameter)
                value = ((ViewModel.Funcs.FuncParameter)value).Value;

            EzSbyt.FormatTypes format;
            if (Enum.TryParse<EzSbyt.FormatTypes>(value.ToString(), out format) == false)
                return DependencyProperty.UnsetValue;

            if (targetType == typeof(Object))
                return format;
            else
                return format.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(ViewModel.Funcs.FuncParameter))
            {
                ViewModel.Funcs.FuncParameter funcParam = new ViewModel.Funcs.FuncParameter(null, value.ToString());
                return funcParam;
            }
            else
                return DependencyProperty.UnsetValue;
        }
    }
}