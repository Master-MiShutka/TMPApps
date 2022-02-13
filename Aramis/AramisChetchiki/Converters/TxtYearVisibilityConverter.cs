using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.WORK.AramisChetchiki.Converters
{
    public class TxtYearVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 3)
            {
                double minValue = values[0] == null ? 0.0 : System.Convert.ToDouble(values[0]);
                double txtWidth = (values[1] == null || values[1] == DependencyProperty.UnsetValue) ? 0.0 : System.Convert.ToDouble(values[1]);
                double panelWidth = (values[2] == null || values[2] == DependencyProperty.UnsetValue) ? 0.0 : System.Convert.ToDouble(values[2]);

                if (panelWidth <= minValue * 2)
                    return Visibility.Collapsed;

                if (double.IsNaN(txtWidth) || txtWidth >= panelWidth)
                    return Visibility.Collapsed;

                if (txtWidth > 0)
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
