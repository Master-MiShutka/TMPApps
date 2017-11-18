using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;


namespace TMP.ARMTES.Converters
{
    public class ProfileToSelectedStartDateConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime now = DateTime.Now;

            if (value == null) return now;

            if (Enum.IsDefined(typeof(ProfileType), value))
            {
                ProfileType profile = (ProfileType)Enum.Parse(typeof(ProfileType), value.ToString());               

                switch (profile)
                {
                    case ProfileType.Current:
                        return now;
                    case ProfileType.Days:
                        return new DateTime(now.Year, now.Month, 1);
                    case ProfileType.Months:
                        {
                            if (now.Month == 1)
                                return new DateTime(now.Year-1, 12, 1);
                            else
                                return new DateTime(now.Year, now.Month-1, 1);
                        }
                }
                return now;
            }
            else
                return now;                      
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("This is a one-way value converter. ConvertBack method is not supported.");
        }

        #endregion
    }

    public class ProfileToSelectedEndDateConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime now = DateTime.Now;

            if (value == null) return now;

            if (Enum.IsDefined(typeof(ProfileType), value))
            {
                ProfileType profile = (ProfileType)Enum.Parse(typeof(ProfileType), value.ToString());
                
                int day = DateTime.DaysInMonth(now.Year, now.Month);

                switch (profile)
                {
                    case ProfileType.Current:
                        return now;
                    case ProfileType.Days:
                        return now;
                }
                return new DateTime(now.Year, now.Month, 1);
            }
            else
                return now;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return DateTime.Now;
        }

        #endregion
    }
}
