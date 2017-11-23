using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.WORK.AramisChetchiki.Converters
{
    public class AbonentBindingInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Model.AbonentBindingNode node = value as Model.AbonentBindingNode;
            if (node == null)
                return DependencyProperty.UnsetValue;
            else
            {
                StackPanel result = new StackPanel() { Orientation = Orientation.Horizontal };
                var obj = node;
                while (obj != null)
                {
                    result.Children.Insert(0, new TextBlock() { Text = "\\", Margin = new Thickness(2, 0, 2, 0) });
                    result.Children.Insert(0, new TextBlock() { Text = obj.Header, Margin = new Thickness(2, 0, 2, 0) });
                    result.Children.Insert(0, new Image() { Source = obj.Icon as System.Windows.Media.ImageSource });
                    obj = obj.Parent as Model.AbonentBindingNode;
                }
                return result;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
