namespace TMPApplication.Converters
{
    using System;
    using System.Windows.Data;

    public sealed class IsNullConverter : IValueConverter
    {
        private static IsNullConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static IsNullConverter()
        {
        }

        private IsNullConverter()
        {
        }

        public static IsNullConverter Instance => instance ?? (instance = new IsNullConverter());

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null == value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
