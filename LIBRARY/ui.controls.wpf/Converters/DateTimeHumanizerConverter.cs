namespace TMP.UI.Controls.WPF.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;

    public class DateTimeHumanizerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            DateTime dateTime;

            if (DateTime.TryParse(value.ToString(), out dateTime))
            {
                const int SECOND = 1;
                const int MINUTE = 60 * SECOND;
                const int HOUR = 60 * MINUTE;
                const int DAY = 24 * HOUR;
                const int MONTH = 30 * DAY;

                var ts = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
                double delta = Math.Abs(ts.TotalSeconds);

                if (delta < 1 * MINUTE)
                {
                    return ts.Seconds == 1 ? "одну секунду назад" : ts.Seconds + " секунд назад";
                }

                if (delta < 2 * MINUTE)
                {
                    return "минуту назад";
                }

                if (delta < 45 * MINUTE)
                {
                    return ts.Minutes + " минут назад";
                }

                if (delta < 90 * MINUTE)
                {
                    return "около часа назад";
                }

                if (delta < 24 * HOUR)
                {
                    return ts.Hours + " часов назад";
                }

                if (delta < 48 * HOUR)
                {
                    return "вчера";
                }

                if (delta < 30 * DAY)
                {
                    return ts.Days + " дней назад";
                }

                if (delta < 12 * MONTH)
                {
                    int months = System.Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                    return months <= 1 ? "месяц назад" : months + " месяцев назад";
                }
                else
                {
                    int years = System.Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                    return years <= 1 ? "год назад" : years + " лет назад";
                }
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
