namespace TMP.WORK.AramisChetchiki.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    public class AbonentBindingInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Model.AbonentBindingNode node)
            {
                return DependencyProperty.UnsetValue;
            }
            else
            {
                StackPanel result = new () { Orientation = Orientation.Horizontal };
                var obj = node;
                while (obj != null)
                {
                    result.Children.Insert(0, new TextBlock() { Text = "\\", Margin = new Thickness(2, 0, 2, 0) });
                    result.Children.Insert(0, new TextBlock() { Text = obj.Header, Margin = new Thickness(2, 0, 2, 0) });
                    result.Children.Insert(0, new Image() { Source = obj.Icon as System.Windows.Media.ImageSource, Height = (obj.Icon as System.Windows.Media.ImageSource).Height });
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
