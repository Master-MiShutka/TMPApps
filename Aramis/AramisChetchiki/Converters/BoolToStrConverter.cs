namespace TMP.WORK.AramisChetchiki.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class BoolToStrConverter : IValueConverter
    {
        public string TrueContent { get; set; } = "да";

        public string FalseContent { get; set; } = "нет";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
            {
                return flag ? this.TrueContent : this.FalseContent;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
