namespace TMP.WORK.AramisChetchiki.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class EnumFlagToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var checkValue = values[0];
            var flags = values[1];

            if (checkValue is Enum && flags is Enum)
            {
                return ((Enum)flags).HasFlag((Enum)checkValue);
            }

            return (object)false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
