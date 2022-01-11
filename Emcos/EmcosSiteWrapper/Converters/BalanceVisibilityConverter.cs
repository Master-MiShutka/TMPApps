using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TMP.Work.Emcos.Model;

namespace TMP.Work.Emcos.Converters
{
    public class BalanceVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IHierarchicalEmcosPoint point = value as IHierarchicalEmcosPoint;
            if (point == null) return DependencyProperty.UnsetValue;
            string param = parameter.ToString();

            switch (param)
            {
                // имеется ли корректировка данных
                case "DirectedEnergyHasCorrection":

                    return Visibility.Visible;
                
                // получены данные не за все сутки
                case "DirectedEnergyNotFullData":
                    return Visibility.Visible;

                // данные за месяц не равны сумме суточных
                case "MonthAndSumDaysNotEqual":
                    return Visibility.Visible;
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
