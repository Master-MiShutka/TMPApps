namespace ItemsFilter.View
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    /// Provide IValueConverter for common ValueTypes.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(string))]
    [ValueConversion(typeof(string), typeof(string))]
    [ValueConversion(typeof(int), typeof(string))]
    [ValueConversion(typeof(long), typeof(string))]
    [ValueConversion(typeof(long), typeof(string))]
    [ValueConversion(typeof(double), typeof(string))]
    [ValueConversion(typeof(Enum), typeof(string))]
    [ValueConversion(typeof(bool), typeof(string))]
    public class SimplePropertyConverter : IValueConverter
    {
        private static SimplePropertyConverter @this = new SimplePropertyConverter();

        private static CultureInfo GetCulture(FrameworkElement element)
        {
            CultureInfo culture;
            if (element != null && DependencyPropertyHelper.GetValueSource(element, FrameworkElement.LanguageProperty).BaseValueSource != BaseValueSource.Default)
            {
                culture = GetCultureInfo(element);
            }
            else
            {
                culture = CultureInfo.CurrentCulture;
            }

            return culture;
        }

        private static CultureInfo GetCultureInfo(DependencyObject element)
        {
            XmlLanguage language = (XmlLanguage)element.GetValue(FrameworkElement.LanguageProperty);
            try
            {
                return language.GetSpecificCulture();
            }
            catch (InvalidOperationException)
            {
                // We default to en-US if no part of the language tag is recognized.
                return CultureInfo.ReadOnly(new CultureInfo("en-us", false));
            }
        }

        public static SimplePropertyConverter This => @this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            try
            {
                if (converter.CanConvertTo(null, typeof(string)))
                {
                    // return converter.ConvertTo(value, typeof(string));
                    return converter.ConvertTo(null, CultureInfo.CurrentCulture, value, typeof(string));
                }
                else
                {
                    return value.ToString();
                }
            }
            catch (Exception)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            try
            {

                if (converter.CanConvertFrom(null, value.GetType()))
                {
                    return converter.ConvertFrom(null, CultureInfo.CurrentCulture, value);
                }
                else
                {
                    return converter.ConvertFrom(value.ToString());
                }
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}
