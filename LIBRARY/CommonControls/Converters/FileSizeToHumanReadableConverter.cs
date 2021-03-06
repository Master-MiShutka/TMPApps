﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Wpf.CommonControls.Converters
{
    public class FileSizeToHumanReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            string[] sizes = { "байт", "килобайт", "мегабайт", "гигабайт" };
            double length = 0d;
            Double.TryParse(value.ToString(), out length);
            int order = 0;
            while (length >= 1024d && order + 1 < sizes.Length)
            {
                order++;
                length = length / 1024d;
            }
            return String.Format("{0:0.##} {1}", length, sizes[order]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
