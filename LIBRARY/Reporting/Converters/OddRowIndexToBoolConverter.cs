using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace TMP.UI.Controls.WPF.Reporting.Converters
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class OddRowIndexToBoolConverter : IValueConverter
    {
        #region Singleton Property

        public static OddRowIndexToBoolConverter Singleton
        {
            get
            {
                if (mg_singleton == null)
                    mg_singleton = new OddRowIndexToBoolConverter();

                return mg_singleton;
            }
        }

        private static OddRowIndexToBoolConverter mg_singleton;

        #endregion Singleton Property

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((!targetType.IsAssignableFrom(typeof(bool)))
              || (value == null)
              || (value.GetType() != typeof(int)))
            {
                return DependencyProperty.UnsetValue;
            }

            int index = (int)value;

            return ((index % 2) == 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
