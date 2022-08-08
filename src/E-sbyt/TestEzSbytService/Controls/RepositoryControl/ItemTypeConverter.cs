using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Controls.RepositoryControl
{
    public class ItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            string typ = parameter as string;
            if (string.IsNullOrEmpty(typ))
                return DependencyProperty.UnsetValue;

            if (typ == "ITEM")
                return "Запись";
            if (typ == "GROUP")
                return "Группа";

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
