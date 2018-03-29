using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TMP.UI.Controls.WPF.Converters
{
  public class MultibindingStringFormatConverter : IMultiValueConverter
  {
    #region IMultiValueConverter Members

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (parameter == null) return "ConverterParameter not be null";
      if (parameter is string == false) return "ConverterParameter must be string";
      string _format = (string)parameter;
      return String.Format(_format, values);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException("This is a one-way value converter. ConvertBack method is not supported.");
    }

    #endregion
  }
}