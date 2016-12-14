using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.Emcos.Converters
{
    public class BalansItemDayPercentValueConverter : IMultiValueConverter
    {
        double ParentHeight { get; set; }
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var d = new Nullable<double>((double)values[0]);
            double percentValue = ((d != null && Double.IsNaN(d.Value) == false) ? d.Value : 0.0);
            double parentHeight = (values[1] != DependencyProperty.UnsetValue ? (double)values[1] : 0.0);

            return percentValue * (parentHeight - 7);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
