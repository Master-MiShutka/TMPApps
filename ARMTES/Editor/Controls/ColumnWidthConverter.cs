﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TMP.ARMTES.Editor
{
    public class ColumnWidthConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.GridLength columnWidth = values[0] is System.Windows.GridLength ? (System.Windows.GridLength)values[0] : new System.Windows.GridLength(0);
            System.Windows.Controls.ScrollBarVisibility vsv = (System.Windows.Controls.ScrollBarVisibility)values[1];

            double width = columnWidth.Value;

            if (vsv == System.Windows.Controls.ScrollBarVisibility.Hidden)
            {
                return new System.Windows.GridLength(width);
            }
            else
            {

                return new System.Windows.GridLength(width - SystemParameters.VerticalScrollBarWidth - 1);
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("This is a one-way value converter. ConvertBack method is not supported.");
        }

        #endregion
    }
}
