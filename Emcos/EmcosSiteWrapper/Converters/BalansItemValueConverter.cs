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

            switch (bi.ElementType)
            {
                case Model.ElementTypes.POWERTRANSFORMER:
                    bi = value as PowerTransformer;
                    break;
                case Model.ElementTypes.FIDER:
                    bi = value as Fider;
                    break;
                case Model.ElementTypes.UNITTRANSFORMER:
                    bi = value as UnitTransformer;
                    break;
                case Model.ElementTypes.UNITTRANSFORMERBUS:
                    bi = value as UnitTransformerBus;
                    break;
            }
            bool flag = false;
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
                    if (bi is IBalansGroup)
                    {
                        GroupItem item = bi as GroupItem;
                        if (item != null)
                            flag = item.DifferenceBetweenDailySumAndMonthPlus > 1.0d;
                        else
                            flag = false;
                    }
                    else
                        flag = bi.DifferenceBetweenDailySumAndMonthPlus > 1.0d;
                    if (flag)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                case "MinusMonthAndSumDaysNotEqual":
                    if ((bi.MonthEminus - bi.Eminus) > 1.0d)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;

                case "DifferenceBetweenDailySumAndMonthPlusToolTip":
                    if (bi is IBalansGroup)
                    {
                        GroupItem item = bi as GroupItem;
                        if (item != null)
                            return item.DifferenceBetweenDailySumAndMonthPlusToolTip;
                        else
                            return "????";
                    }
                    else
                        return "Сумма суточных значений не равна расходу за месяц";
                case "DifferenceBetweenDailySumAndMonthMinusToolTip":
                    if (bi is IBalansGroup)
                    {
                        GroupItem item = bi as GroupItem;
                        if (item != null)
                            return item.DifferenceBetweenDailySumAndMonthMinusToolTip;
                        else
                            return "????";
                    }
                    else
                        return "Сумма суточных значений не равна расходу за месяц";

                default: return DependencyProperty.UnsetValue;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
