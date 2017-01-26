using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public class BaseConverter
    {
        public object GO<T>(object value)
        {
            if (value.GetType() == typeof(String))
            {
                string data = value as String;
                return App.Base64StringToObject<T>(data);
            }
            else
            {
                T obj = (T)value;
                return App.ObjectToBase64String<T>(obj);
            }
        }
    }

    [ValueConversion(typeof(Model.EmcosReport), typeof(string))]
    public class Base64StringToEmcosReportConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return GO<Model.EmcosReport>(value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return GO<Model.EmcosReport> (value);
        }
    }
    [ValueConversion(typeof(ListPoint), typeof(string))]
    public class Base64StringToListPointConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return GO<ListPoint> (value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return GO<ListPoint>(value);
        }
    }
}