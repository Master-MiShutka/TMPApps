using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Converters
{
  public class CharReplaceConverter : IValueConverter
  {
    public char NewChar { get; set; } = ' ';
    public char OldChar { get; set; } = '_';
    public string Convert(string value)
    {
      if (value != null)
        return value.Replace(OldChar, NewChar);
      else
        return null;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null)
        return value.ToString().Replace(OldChar, NewChar);
      else
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }
}