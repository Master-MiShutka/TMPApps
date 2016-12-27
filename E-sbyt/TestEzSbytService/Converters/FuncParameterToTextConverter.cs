using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Converters
{
    public class FuncParameterToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            ViewModel.Funcs.FuncParameter funcParameter = (ViewModel.Funcs.FuncParameter)value;
            return funcParameter.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new ViewModel.Funcs.FuncParameter(null, value.ToString());
        }
    }
}