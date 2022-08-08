using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using TMP.Work.Emcos.Model.Balance;

namespace TMP.Work.Emcos.Converters
{
    public class DaysValuesListTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IDirectedEnergy energy = (IDirectedEnergy)value;

            if (energy == null)
                return DependencyProperty.UnsetValue;

            if (energy.SummOfDaysValue == null &&
                energy.DaysValuesAverage == null &&
                energy.DaysValuesMin == null &&
                energy.DaysValuesMax == null &&
                energy.DaysValuesWithStatus == null)
                return DependencyProperty.UnsetValue;

            return string.Format("Сумма: {0:n2}\nСреднее: {1:n2}\nМинимальное: {2:n2}\nМаксимальное: {3:n2}\nОтсутствующих данных: {4}", 
                energy.SummOfDaysValue ?? 0, 
                energy.DaysValuesAverage ?? 0, 
                energy.DaysValuesMin ?? 0, 
                energy.DaysValuesMax ?? 0, 
                energy.DaysValuesWithStatus.Count(day => day.Status == DataValueStatus.Missing));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
