﻿namespace TMP.WORK.AramisChetchiki.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    public class PassThroughConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToList();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}