namespace TMP.UI.Controls.WPF.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class LevelToIndentConverter : IValueConverter
    {
        public object Convert(object o, Type type, object parameter,
                              CultureInfo culture)
        {
            return new Thickness((int)o * c_IndentSize, 0, 0, 0);
        }

        public object ConvertBack(object o, Type type, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private const double c_IndentSize = 19.0;
    }

    public class CanExpandConverter : IValueConverter
    {
        public object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            if ((bool)o)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
