namespace TMP.WORK.AramisChetchiki.Converters
{
    using System;
    using System.Globalization;
    using TMP.UI.WPF.Controls.Converters;

    public class FieldNameConverter : MarkupConverter
    {
        private static FieldNameConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static FieldNameConverter()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new FieldNameConverter());
        }

        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is not string str)
            {
                return string.Empty;
            }
            else
            {
                return Utils.ConvertFromTitleCase(value as string);
            }
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
