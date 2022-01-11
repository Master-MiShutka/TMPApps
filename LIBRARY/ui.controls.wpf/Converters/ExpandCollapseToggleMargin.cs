namespace TMP.UI.Controls.WPF.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class ExpandCollapseToggleMargin : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double height = (double)values[0];
            if (double.IsNaN(height) || height == 0)
            {
                return new Thickness(1d);
            }

            bool expanded = (bool)values[1];
            Thickness margin = new Thickness(2d);
            if (expanded)
            {
                double borderMargin = 0;
                double borderThickness = 1;
                double lineThickness = 2;

                double top = (height - (borderThickness * 2) - (borderMargin * 2) - lineThickness) / 2;
                return new Thickness(2, top, 2, top);
            }

            return margin;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
