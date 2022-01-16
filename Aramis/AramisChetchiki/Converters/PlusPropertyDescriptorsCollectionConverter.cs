namespace TMP.WORK.AramisChetchiki.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using TMP.Shared;

    public class PlusPropertyDescriptorsCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Model.TableViewKinds viewKind)
            {
                return ModelHelper.GetMeterFieldsCollectionByTableViewKind(viewKind);
            }
            else
            if (value is PlusPropertyDescriptorsCollection collection)
            {
                return this.ConvertToFields(collection);
            }
            else
            if (value is ICollection<string> fields)
            {
                return this.ConvertToProperties(fields);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlusPropertyDescriptorsCollection collection)
            {
                return this.ConvertToFields(collection);
            }
            else
            if (value is ICollection<string> fields)
            {
                return this.ConvertToProperties(fields);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        private IList ConvertToFields(PlusPropertyDescriptorsCollection collection)
        {
            Type type = collection.FirstOrDefault().ModelType;

            IList result = type switch
            {
                Type t when t == typeof(Model.Meter) => new MeterFieldsCollection(),
                Type t when t == typeof(Model.ChangeOfMeter) => new ChangesOfMetersFieldsCollection(),
                Type t when t == typeof(Model.SummaryInfoItem) => new SummaryInfoFieldsCollection(),
                _ => throw new NotImplementedException($"Unknown type '{type}'"),
            };

            foreach (PlusPropertyDescriptor plusProperty in collection)
            {
                result.Add(plusProperty.Name);
            }

            return result;
        }

        private PlusPropertyDescriptorsCollection ConvertToProperties(ICollection<string> collection)
        {
            PlusPropertyDescriptorsCollection result = new ();

            if (collection == null)
                return result;

            IDictionary<string, PlusPropertyDescriptor> dictionary = collection switch
            {
                SummaryInfoFieldsCollection => ModelHelper.MeterSummaryInfoPropertiesCollection,
                ChangesOfMetersFieldsCollection => ModelHelper.ChangesOfMetersPropertiesCollection,
                MeterFieldsCollection => ModelHelper.MeterPropertiesCollection,

                _ => throw new NotImplementedException($"Unknown type '{collection.GetType()}'"),
            };

            foreach (string item in collection)
            {
                if (dictionary.ContainsKey(item))
                {
                    result.Add(dictionary[item]);
                }
            }

            return result;
        }
    }
}
