using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.Emcos.Converters
{
    using TMP.Work.Emcos.Model.Balans;
    public class BalansItemValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = parameter.ToString();
            if (String.IsNullOrWhiteSpace(param)) return DependencyProperty.UnsetValue;

            var bi = value as IBalansItem;
            if (bi == null)
                return DependencyProperty.UnsetValue;
            bool isItem = false;
            switch (bi.Type)
            {
                case Model.ElementTypes.PowerTransformer:
                    bi = value as PowerTransformer;
                    isItem = true;
                    break;
                case Model.ElementTypes.Fider:
                    bi = value as Fider;
                    isItem = true;
                    break;
                case Model.ElementTypes.UnitTransformer:
                    bi = value as UnitTransformer;
                    isItem = true;
                    break;
                case Model.ElementTypes.UnitTransformerBus:
                    bi = value as UnitTransformerBus;
                    isItem = true;
                    break;
            }
            switch (param)
            {
                case "ValuePlusStatus":
                    if (bi.NotFullDataPlus)
                        if (bi.DataPlusStatus == null)
                            return "Неполные данные!";
                        else
                            return "Неполные данные!\nКоличество отсутствующих: " + bi.DataPlusStatus;
                    else
                        return DependencyProperty.UnsetValue;
                case "ValueMinusStatus":
                    if (bi.NotFullDataMinus)
                        if (bi.DataMinusStatus == null)
                            return "Неполные данные!";
                        else
                            return "Неполные данные!\nКоличество отсутствующих: " + bi.DataMinusStatus;
                    else
                        return DependencyProperty.UnsetValue;
                case "ExcessUnbalance":
                    if ((bi is Substation) || (bi is SubstationSection))
                    {
                        var sc = bi as SubstationSection;
                        if (sc != null && sc.ExcessUnbalance) return true;
                        var s = bi as Substation;
                        if (s != null && s.ExcessUnbalance) return true;
                        return false;
                    }
                    else
                        return DependencyProperty.UnsetValue;
                case "SetBoldValuesOnGroup":
                    if (bi is IBalansGroup)
                        return FontWeights.SemiBold;
                    else
                        return FontWeights.Normal;
                case "PlusMonthAndSumDaysNotEqual":
                    if ((bi.MonthEplus - bi.Eplus) > 1.0d)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                case "MinusMonthAndSumDaysNotEqual":
                    if ((bi.MonthEminus - bi.Eminus) > 1.0d)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                default: return DependencyProperty.UnsetValue;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
