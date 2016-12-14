using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.Emcos.Converters
{
    public class BalansItemVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = parameter == null ? null : parameter.ToString();
            var item = value as Model.Balans.IBalansItem;
            switch (param)
            {
                case null:
                case "Visibility":
                    if (value == null)
                        return Visibility.Collapsed;
                    if (item == null)
                        return Visibility.Collapsed;
                    if (item.Type == Model.ElementTypes.Fider ||
                        item.Type == Model.ElementTypes.PowerTransformer ||
                        item.Type == Model.ElementTypes.UnitTransformer ||
                        item.Type == Model.ElementTypes.UnitTransformerBus)
                    {
                        return Visibility.Visible;
                    }
                    else
                        return Visibility.Collapsed;
                case "Bool":
                    if (value == null)
                        return false;
                    if (item == null)
                        return false;
                    if (item.Type == Model.ElementTypes.Fider ||
                        item.Type == Model.ElementTypes.PowerTransformer ||
                        item.Type == Model.ElementTypes.UnitTransformer ||
                        item.Type == Model.ElementTypes.UnitTransformerBus)
                    {
                        return true;
                    }
                    else
                        return false;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
