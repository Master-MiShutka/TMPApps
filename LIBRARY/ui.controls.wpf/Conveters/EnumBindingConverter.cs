using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace TMP.UI.Controls.WPF.Converters
{
    public class EnumBindingConverter : MarkupExtension, IValueConverter
    {
        private class LocalDictionaries
        {
            public readonly Dictionary<int, string> Descriptions = new Dictionary<int, string>();
            public readonly Dictionary<string, int> IntValues = new Dictionary<string, int>();
            public IEnumerable<string> ItemsSource;
        }

        private readonly Dictionary<Type, LocalDictionaries> _localDictionaries = new Dictionary<Type, LocalDictionaries>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !value.GetType().IsEnum)
                return value;
            if (!_localDictionaries.ContainsKey(value.GetType()))
                CreateDictionaries(value.GetType());
            //This is for the ItemsSource
            if (targetType == typeof(IEnumerable))
                return _localDictionaries[value.GetType()].ItemsSource;

            //Normal SelectedItem case where it exists
            if (_localDictionaries[value.GetType()].Descriptions.ContainsKey((int)value))
                return _localDictionaries[value.GetType()].Descriptions[(int)value];

            //Have to handle 0 case, else an issue
            if ((int)value == 0) return null;

            //Error condition
            throw new InvalidEnumArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || (!targetType.IsEnum && !targetType.IsGenericType))
                return value;
            if (targetType.IsGenericType)
                targetType = Nullable.GetUnderlyingType(targetType);
            if (!_localDictionaries.ContainsKey(targetType))
                CreateDictionaries(targetType);
            int enumInt = _localDictionaries[targetType].IntValues[value.ToString()];
            return Enum.ToObject(targetType, enumInt);
        }

        private void CreateDictionaries(Type e)
        {
            var dictionaries = new LocalDictionaries();

            foreach (var value in Enum.GetValues(e))
            {
                FieldInfo info = value.GetType().GetField(value.ToString());
                var valueDescription = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (valueDescription.Length == 1)
                {
                    dictionaries.Descriptions.Add((int)value, valueDescription[0].Description);
                    dictionaries.IntValues.Add(valueDescription[0].Description, (int)value);
                }
                else //Use the value for display if not concrete result
                {
                    dictionaries.Descriptions.Add((int)value, value.ToString());
                    dictionaries.IntValues.Add(value.ToString(), (int)value);
                }
            }
            dictionaries.ItemsSource = dictionaries.Descriptions.Select(i => i.Value);
            _localDictionaries.Add(e, dictionaries);
        }

        public override object ProvideValue(IServiceProvider serviceProvider) { return this; }
    }
}