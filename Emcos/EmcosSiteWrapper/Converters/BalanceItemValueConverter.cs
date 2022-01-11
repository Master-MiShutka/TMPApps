using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.Emcos.Converters
{
    using TMP.Work.Emcos.Model.Balance;
    public class BalanceItemValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = parameter.ToString();
            if (String.IsNullOrWhiteSpace(param)) return DependencyProperty.UnsetValue;

            var bi = value as IBalanceItem;
            if (bi == null)
                return DependencyProperty.UnsetValue;

            if (param == "SetBoldValuesOnGroup")
            {
                if (bi is IBalanceGroupItem)
                    return FontWeights.SemiBold;
                else
                    return FontWeights.Normal;
            }




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
                    if (bi.ActiveEnergy.Plus.NotFullData)
                        if (bi.ActiveEnergy.Plus.DaysValuesStatus == null)
                            return "Неполные данные!";
                        else
                            return "Неполные данные!\nКоличество отсутствующих: " + bi.ActiveEnergy.Plus.DaysValuesStatus;
                    else
                        return DependencyProperty.UnsetValue;
                case "ValueMinusStatus":
                    if (bi.ActiveEnergy.Minus.NotFullData)
                        if (bi.ActiveEnergy.Minus.DaysValuesStatus == null)
                            return "Неполные данные!";
                        else
                            return "Неполные данные!\nКоличество отсутствующих: " + bi.ActiveEnergy.Minus.DaysValuesStatus;
                    else
                        return DependencyProperty.UnsetValue;
                case "ExcessUnbalance":
                    if ((bi is Substation) || (bi is SubstationSection))
                    {
                        var sc = bi as SubstationSection;
                        if (sc != null && sc.ActiveEnergyBalance.ExcessUnbalance) return true;
                        var s = bi as Substation;
                        if (s != null && s.ActiveEnergyBalance.ExcessUnbalance) return true;
                        return false;
                    }
                    else
                        return DependencyProperty.UnsetValue;

                case "PlusMonthAndSumDaysNotEqual":
                    if (bi is IBalanceGroupItem)
                    {
                        BalanceGroupItem item = bi as BalanceGroupItem;
                        if (item != null)
                            flag = item.ActiveEnergy.Plus.HasDifferenceBetweenDaysSumAndMonth;
                        else
                            flag = false;
                    }
                    else
                        flag = bi.ActiveEnergy.Plus.HasDifferenceBetweenDaysSumAndMonth;
                    if (flag)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                case "MinusMonthAndSumDaysNotEqual":
                    if (bi.ActiveEnergy.Minus.HasDifferenceBetweenDaysSumAndMonth)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;

                case "DifferenceBetweenDailySumAndMonthPlusToolTip":
                    if (bi is IBalanceGroupItem)
                    {
                        BalanceGroupItem item = bi as BalanceGroupItem;
                        if (item != null)
                            return item.ActiveEnergy.Plus.DifferenceBetweenDaysSumAndMonth;
                        else
                            return "????";
                    }
                    else
                        return "Сумма суточных значений не равна расходу за месяц";
                case "DifferenceBetweenDailySumAndMonthMinusToolTip":
                    if (bi is IBalanceGroupItem)
                    {
                        BalanceGroupItem item = bi as BalanceGroupItem;
                        if (item != null)
                            return item.ActiveEnergy.Minus.DifferenceBetweenDaysSumAndMonth;
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
