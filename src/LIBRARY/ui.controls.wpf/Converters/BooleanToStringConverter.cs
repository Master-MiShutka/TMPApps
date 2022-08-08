namespace TMP.UI.WPF.Controls.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    public class BooleanToValueConverter<T> : IValueConverter
    {
        public T TrueValue { get; set; }

        public T FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? this.FalseValue : ((bool)value ? this.TrueValue : this.FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Note: this implementation precludes the use of "null" as the
            // value for TrueValue. Probably not an issue in 99.94% of all cases,
            // but something to consider, if one is looking to make a truly 100%
            // general-purpose class here.
            return value != null && EqualityComparer<T>.Default.Equals((T)value, this.TrueValue);
        }
    }

    public class BooleanToStringConverter : BooleanToValueConverter<string>
    {
    }
}
