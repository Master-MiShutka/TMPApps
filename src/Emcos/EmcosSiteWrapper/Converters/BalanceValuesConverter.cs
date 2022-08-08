using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TMP.Work.Emcos.Model;

namespace TMP.Work.Emcos.Converters
{
    public class BalanceValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IHierarchicalEmcosPoint point = value as IHierarchicalEmcosPoint;

            if (point == null) return DependencyProperty.UnsetValue;

            if (point.IsGroup && point.HasChildren == false)
            {
                return 123.45;
            }

            string param = parameter.ToString();

            switch (param)
            {
                case "PlusEnergy":
                    break;
                case "MinusEnergy":
                    break;


                case "DirectedEnergyCorrection":
                    break;
                case "ValueStatus":
                    break;


                case "Unbalance":
                    break;
                        case "PercentageOfUnbalance":
                    break;
                case "Description":
                    break;
                case "Correction":
                    break;

                case "ExcessUnbalance":
                    break;

                case "MaximumAllowableUnbalance":
                    break;

                // подсказка при неравенстве суммы данных за сутки и за месяц
                case "DifferenceBetweenDailySumAndMonthToolTip":
                    break;

                default:
                    break;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
