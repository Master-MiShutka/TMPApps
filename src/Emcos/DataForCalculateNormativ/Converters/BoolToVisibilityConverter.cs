using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Negative { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(bool))
            {
                bool flag = (bool)value;

                Visibility result = Visibility.Collapsed;

                if (flag)
                    result = Visibility.Visible;

                if (Negative)
                {
                    if (result == Visibility.Visible)
                        result = Visibility.Collapsed;
                    else
                        result = Visibility.Visible;
                }

                return result;
            }
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}