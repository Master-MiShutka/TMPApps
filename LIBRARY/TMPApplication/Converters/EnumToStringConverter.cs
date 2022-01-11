namespace TMPApplication.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using TMP.Shared;

    public sealed class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            EnumDescriptionTypeConverter enumDescriptionTypeConverter = new EnumDescriptionTypeConverter(value.GetType());

            return enumDescriptionTypeConverter.ConvertTo(null, null, value, typeof(string));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
