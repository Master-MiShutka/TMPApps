using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace TMP.Work.Emcos.Converters
{
    public class DaysValuesListTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var values = (IList<Model.Balans.DataValue>)value;

            if (values == null)
                return DependencyProperty.UnsetValue;

            double sum = values.Sum((i) => i.DoubleValue == null ? 0 : i.DoubleValue.Value);
            double av = values.Average((i) => i.DoubleValue == null ? 0 : i.DoubleValue.Value);
            double min = values.Min((i) => i.DoubleValue == null ? 0 : i.DoubleValue.Value);
            double max = values.Max((i) => i.DoubleValue == null ? 0 : i.DoubleValue.Value);

            int missing = values.Count((i) => i.Status == Model.Balans.DataValueStatus.Missing);

            return string.Format("Сумма: {0:n2}\nСреднее: {1:n2}\nМинимальное: {2:n2}\nМаксимальное: {3:n2}\nОтсутствующих данных: {4}", sum, av, min, max, missing);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
