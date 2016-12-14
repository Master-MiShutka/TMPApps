using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TMP.ARMTES.Converters
{
    public class ObjectToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool reverse = false;
            if (parameter != null) Boolean.TryParse(parameter.ToString(), out reverse);

            if (value == null)
                if (reverse == true)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;

            else
                if (reverse == true)
                    return Visibility.Collapsed;
                 else
                    return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("This is a one-way value converter. ConvertBack method is not supported.");
        }

        #endregion
    }
}
