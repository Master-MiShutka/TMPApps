using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Converters
{
    public class CollectionViewNavigatorPosConverter : IMultiValueConverter
    {
        public bool IncreaseFirstValueOverOne { get; set; } = false;
        public bool IncreaseSecondValueOverOne { get; set; } = false;

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int _curPage = values[0] is int ? (int)values[0] : 0;
            int _pageCount = values[1] is int ? (int)values[1] : 0;

            string _format = parameter is string ? (string)parameter : "???";

            if (IncreaseFirstValueOverOne)
                _curPage++;
            if (IncreaseSecondValueOverOne)
                _pageCount++;

            return String.Format(_format, _curPage, _pageCount);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("This is a one-way value converter. ConvertBack method is not supported.");
        }

        #endregion
    }
}