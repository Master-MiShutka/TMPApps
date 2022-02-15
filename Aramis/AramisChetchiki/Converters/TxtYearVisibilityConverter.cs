using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.WORK.AramisChetchiki.Converters
{
    public class TxtYearVisibilityConverter : IMultiValueConverter
    {
        public double FontSize { get; set; }

        private double txtWidth;

        public TxtYearVisibilityConverter()
        {
            try
            {
                var formattedText = new System.Windows.Media.FormattedText(
                    "2222",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface("Verdana"),
                    this.FontSize == 0 ? 14d : this.FontSize,
                    System.Windows.Media.Brushes.Black,
                    new System.Windows.Media.NumberSubstitution(),
                    1);

                this.txtWidth = formattedText.Width;
            }
            catch (Exception e)
            {
                ;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                double minValue = values[0] == null ? 0.0 : System.Convert.ToDouble(values[0]);
                double panelWidth = (values[1] == null || values[1] == DependencyProperty.UnsetValue) ? 0.0 : System.Convert.ToDouble(values[1]);

                if (panelWidth > this.txtWidth)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
