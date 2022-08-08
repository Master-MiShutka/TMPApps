using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using TMP.DWRES.ViewModel;

namespace TMP.DWRES.Converters
{
    [ValueConversion(typeof(StateType), typeof(Visibility))]
    public class StateTypeToVisibilitylConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StateType state = (StateType)Enum.Parse(typeof(StateType), value.ToString());
            if (state == StateType.Ready)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
