namespace TMP.UI.Controls.WPF.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;

    public class NullToUnsetValueConverter : MarkupConverter
    {
        private static NullToUnsetValueConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static NullToUnsetValueConverter()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new NullToUnsetValueConverter());
        }

        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? DependencyProperty.UnsetValue;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
