using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace TMP.WORK.AramisChetchiki.Utils
{
    using TMP.Shared.Common;
    using TMP.Shared.Windows.Collections;
    using TMP.WORK.AramisChetchiki.DbModel;
    using TMP.WORK.AramisChetchiki.Model;

    public static class ModelHelper
    {
        public static void Init()
        {
            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() =>
            {
                IList<string> list1 = MeterPropertiesNames;
                IDictionary<string, PlusPropertyDescriptor> list2 = MeterPropertiesCollection;
                Dictionary<string, PropertyInfo> list3 = MeterProperties;
                Dictionary<string, string> list4 = MeterPropertyDisplayNames;
                IList<string> list5 = MeterSummaryInfoProperties;
                IDictionary<string, PlusPropertyDescriptor> list6 = MeterSummaryInfoPropertiesCollection;

                IEnumerable<string> list7 = ChangesOfMeterPropertiesNames;
                IDictionary<string, PlusPropertyDescriptor> list8 = ChangesOfMetersPropertiesCollection;
                Dictionary<string, PropertyInfo> list9 = ChangesOfMetersProperties;

                IEnumerable<string> list10 = ElectricitySupplyPropertiesNames;
                IDictionary<string, PlusPropertyDescriptor> list11 = ElectricitySupplyPropertiesCollection;
                Dictionary<string, PropertyInfo> list12 = ElectricitySupplyProperties;

                IEnumerable<string> list13 = PaymentPropertiesNames;
                IDictionary<string, PlusPropertyDescriptor> list14 = PaymentPropertiesCollection;
                Dictionary<string, PropertyInfo> list15 = PaymentProperties;

                IEnumerable<string> list16 = SummaryInfoItemPropertiesNames;
                IDictionary<string, PlusPropertyDescriptor> list17 = SummaryInfoItemPropertiesCollection;
                Dictionary<string, PropertyInfo> list18 = SummaryInfoItemProperties;

                IEnumerable<string> list19 = FiderAnalizMeterPropertiesNames;
                IDictionary<string, PlusPropertyDescriptor> list20 = FiderAnalizMeterPropertiesCollection;
                Dictionary<string, PropertyInfo> list21 = FiderAnalizMeterProperties;
            });
        }

        #region | Meter |

        private static IList<string> meterPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> meterPropertiesCollection;
        private static Dictionary<string, PropertyInfo> meterProperties;
        private static Dictionary<string, string> meterPropertyDisplayNames;
        private static IList<string> meterSummaryInfoProperties;
        private static IDictionary<string, PlusPropertyDescriptor> meterSummaryInfoPropertiesCollection;

        public static Type MeterType => typeof(Meter);

        public static ICollection<PlusPropertyDescriptor> MeterPropertyDescriptors => MeterPropertiesCollection?.Values;

        /// <summary>
        /// Список имен свойств типа <see cref="Model.Meter"/>
        /// </summary>
        public static IList<string> MeterPropertiesNames
        {
            get
            {
                if (meterPropertiesNames == null)
                {
                    meterPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in MeterPropertiesCollection.Values)
                    {
                        meterPropertiesNames.Add(prop.Name);
                    }

                    (meterPropertiesNames as List<string>).Sort();
                }

                return meterPropertiesNames;
            }
        }

        /// <summary>
        /// Словарь свойств типа <see cref="Model.Meter"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> MeterPropertiesCollection
        {
            get
            {
                if (meterPropertiesCollection == null)
                {
                    meterPropertiesCollection = new Dictionary<string, PlusPropertyDescriptor>();
                    PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(MeterType);

                    // var sortedCollection = new SortedSet<PlusPropertyDescriptor>(new PropertyDescriptorComparer());

                    List<PlusPropertyDescriptor> list = new List<PlusPropertyDescriptor>();

                    foreach (PropertyDescriptor pd in collection)
                    {
                        // if (sortedCollection.Add(new PlusPropertyDescriptor(pd)) == false)
                        // {
                        // }

                        list.Add(new PlusPropertyDescriptor(pd));
                    }

                    foreach (PlusPropertyDescriptor ppd in list) // sortedCollection)
                    {
                        meterPropertiesCollection.Add(ppd.Name, ppd);
                    }
                }

                return meterPropertiesCollection;
            }
        }

        public static Dictionary<string, PropertyInfo> MeterProperties
        {
            get
            {
                if (meterProperties == null)
                {
                    meterProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in MeterPropertiesNames)
                    {
                        meterProperties.Add(prop, MeterType.GetProperty(prop));
                    }
                }

                return meterProperties;
            }
        }

        public static Dictionary<string, string> MeterPropertyDisplayNames
        {
            get
            {
                if (meterPropertyDisplayNames == null)
                {
                    meterPropertyDisplayNames = new Dictionary<string, string>();
                    foreach (PlusPropertyDescriptor prop in MeterPropertiesCollection.Values)
                    {
                        meterPropertyDisplayNames.Add(prop.Name, prop.DisplayName);
                    }
                }

                return meterPropertyDisplayNames;
            }
        }

        public static IList<string> MeterSummaryInfoProperties
        {
            get
            {
                if (meterSummaryInfoProperties == null)
                {
                    meterSummaryInfoProperties = new List<string>();

                    // атрибут, показывающий о необходимости сбора сводной информации
                    Type summaryInfoAttr = typeof(SummaryInfoAttribute);
                    foreach (PlusPropertyDescriptor pd in MeterPropertiesCollection.Values)
                    {
                        if (pd.Attributes[summaryInfoAttr] != null)
                        {
                            meterSummaryInfoProperties.Add(pd.Name);
                        }
                    }
                }

                return meterSummaryInfoProperties;
            }
        }

        public static IDictionary<string, PlusPropertyDescriptor> MeterSummaryInfoPropertiesCollection
        {
            get
            {
                if (meterSummaryInfoPropertiesCollection == null)
                {
                    meterSummaryInfoPropertiesCollection = new Dictionary<string, PlusPropertyDescriptor>();
                    PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(MeterType);

                    // атрибут, показывающий о необходимости сбора сводной информации
                    Type summaryInfoAttr = typeof(SummaryInfoAttribute);

                    IList<PlusPropertyDescriptor> plusPropsCollection = new List<PlusPropertyDescriptor>();
                    foreach (PropertyDescriptor pd in collection)
                    {
                        if (pd.Attributes[summaryInfoAttr] != null)
                        {
                            plusPropsCollection.Add(new PlusPropertyDescriptor(pd));
                        }
                    }

                    if (plusPropsCollection.All(i => i.Order == 0))
                    {
                        for (int ind = 0; ind < plusPropsCollection.Count; ind++)
                        {
                            plusPropsCollection[ind].Order = ind + 1;
                        }
                    }

                    SortedSet<PlusPropertyDescriptor> sortedCollection = new SortedSet<PlusPropertyDescriptor>(plusPropsCollection, new PropertyDescriptorComparer());

                    foreach (PlusPropertyDescriptor ppd in sortedCollection)
                    {
                        meterSummaryInfoPropertiesCollection.Add(ppd.Name, ppd);
                    }
                }

                return meterSummaryInfoPropertiesCollection;
            }
        }

        public static ICollection<PlusPropertyDescriptor> MeterSummaryInfoItemDescriptors => MeterSummaryInfoPropertiesCollection?.Values;

        public static object MeterGetPropertyValue(Meter meter, string property)
        {
            PropertyInfo pi = MeterProperties[property];
            object value = pi.GetValue(meter, null);
            if (pi.PropertyType == typeof(bool))
            {
                return (bool)value ? "да" : "нет";
            }

            if (pi.PropertyType == typeof(Address))
            {
                return value.ToString();
            }

            return value;
        }

        public static ItemPropertyInfo GetMeterPropertyInfo(string propertyName)
        {
            return new ItemPropertyInfo(propertyName, MeterType, MeterPropertiesCollection[propertyName]);
        }

        #endregion

        #region | FiderAnalizMeter |

        private static Dictionary<string, PropertyInfo> fiderAnalizMeterProperties;
        private static IDictionary<string, PlusPropertyDescriptor> fiderAnalizMeterPropertiesCollection;
        private static IList<string> fiderAnalizMeterPropertiesNames;

        public static Type FiderAnalizMeterType => typeof(FiderAnalizMeter);

        /// <summary>
        /// Список имен свойств типа <see cref="Model.FiderAnalizMeter"/>
        /// </summary>
        public static IList<string> FiderAnalizMeterPropertiesNames
        {
            get
            {
                if (fiderAnalizMeterPropertiesNames == null)
                {
                    fiderAnalizMeterPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in FiderAnalizMeterPropertiesCollection.Values)
                    {
                        fiderAnalizMeterPropertiesNames.Add(prop.Name);
                    }

                    (fiderAnalizMeterPropertiesNames as List<string>).Sort();
                }

                return fiderAnalizMeterPropertiesNames;
            }
        }

        /// <summary>
        /// Словарь свойств типа <see cref="Model.FiderAnalizMeter"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> FiderAnalizMeterPropertiesCollection
        {
            get
            {
                if (fiderAnalizMeterPropertiesCollection == null)
                {
                    fiderAnalizMeterPropertiesCollection = new Dictionary<string, PlusPropertyDescriptor>();
                    PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(FiderAnalizMeterType);

                    List<PlusPropertyDescriptor> list = new List<PlusPropertyDescriptor>();

                    foreach (PropertyDescriptor pd in collection)
                    {
                        list.Add(new PlusPropertyDescriptor(pd));
                    }

                    foreach (PlusPropertyDescriptor ppd in list)
                    {
                        fiderAnalizMeterPropertiesCollection.Add(ppd.Name, ppd);
                    }
                }

                return fiderAnalizMeterPropertiesCollection;
            }
        }

        public static Dictionary<string, PropertyInfo> FiderAnalizMeterProperties
        {
            get
            {
                if (fiderAnalizMeterProperties == null)
                {
                    fiderAnalizMeterProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in FiderAnalizMeterPropertiesNames)
                    {
                        fiderAnalizMeterProperties.Add(prop, FiderAnalizMeterType.GetProperty(prop));
                    }
                }

                return fiderAnalizMeterProperties;
            }
        }

        public static object FiderAnalizMeterGetPropertyValue(FiderAnalizMeter meter, string property)
        {
            PropertyInfo pi = FiderAnalizMeterProperties[property];
            object value = pi.GetValue(meter, null);
            if (pi.PropertyType == typeof(bool))
            {
                return (bool)value ? "да" : "нет";
            }

            if (pi.PropertyType == typeof(Address))
            {
                return value.ToString();
            }

            return value;
        }

        #endregion

        #region | ChangesOfMeters |

        private static readonly object Lock = new();

        public static Type ChangesOfMeterType => typeof(ChangeOfMeter);

        private static IEnumerable<string> changesOfMeterPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> changesOfMeterPropertiesCollection;
        private static Dictionary<string, PropertyInfo> changesOfMeterProperties;

        public static ICollection<PlusPropertyDescriptor> ChangesOfMetersDescriptors => ChangesOfMetersPropertiesCollection?.Values;

        /// <summary>
        /// Словарь имен свойств типа <see cref="Model.ChangeOfMeter"/>
        /// </summary>
        public static IEnumerable<string> ChangesOfMeterPropertiesNames
        {
            get
            {
                if (changesOfMeterPropertiesNames == null)
                {
                    changesOfMeterPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in ChangesOfMetersPropertiesCollection.Values)
                    {
                        ((IList)changesOfMeterPropertiesNames).Add(prop.Name);
                    }

                    (changesOfMeterPropertiesNames as List<string>).Sort();
                }

                return changesOfMeterPropertiesNames;
            }
        }

        /// <summary>
        /// Список свойств типа <see cref="Model.ChangeOfMeter"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> ChangesOfMetersPropertiesCollection
        {
            get
            {
                if (changesOfMeterPropertiesCollection == null)
                {
                    lock (Lock)
                    {
                        Dictionary<string, PlusPropertyDescriptor> result = new();
                        PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(ChangesOfMeterType);

                        System.Diagnostics.Debug.Assert(collection.Count > 0, "TypeDescriptor.GetProperties(ChangesOfMeterType).Count <= 0");

                        IList<PlusPropertyDescriptor> plusPropsCollection = new List<PlusPropertyDescriptor>();
                        foreach (PropertyDescriptor pd in collection)
                        {
                            plusPropsCollection.Add(new PlusPropertyDescriptor(pd));
                        }

                        if (plusPropsCollection.All(i => i.Order == 0))
                        {
                            for (int ind = 0; ind < plusPropsCollection.Count; ind++)
                            {
                                plusPropsCollection[ind].Order = ind + 1;
                            }
                        }

                        SortedSet<PlusPropertyDescriptor> sortedCollection = new SortedSet<PlusPropertyDescriptor>(plusPropsCollection, new PropertyDescriptorComparer());

                        foreach (PlusPropertyDescriptor ppd in sortedCollection)
                        {
                            result.Add(ppd.Name, ppd);
                        }

                        changesOfMeterPropertiesCollection = result;
                    }
                }

                return changesOfMeterPropertiesCollection;
            }
        }

        public static Dictionary<string, PropertyInfo> ChangesOfMetersProperties
        {
            get
            {
                if (changesOfMeterProperties == null)
                {
                    changesOfMeterProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in ChangesOfMeterPropertiesNames)
                    {
                        changesOfMeterProperties.Add(prop, ChangesOfMeterType.GetProperty(prop));
                    }
                }

                return changesOfMeterProperties;
            }
        }

        public static IEnumerable<PlusPropertyDescriptor> GetChangesOfMetersPropertyDescriptors()
        {
            IEnumerable<PlusPropertyDescriptor> fields = GetFields(ChangesOfMetersPropertiesCollection, AppSettings.Default.ChangesOfMetersFields);
            int index = 0;
            foreach (PlusPropertyDescriptor item in fields)
            {
                item.Order = index++;
            }

            return fields.OrderBy(i => i.Order);
        }

        #endregion

        #region | ElectricitySupply |

        public static Type ElectricitySupplyType => typeof(ElectricitySupply);

        private static IEnumerable<string> electricitySupplyPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> electricitySupplyPropertiesCollection;
        private static Dictionary<string, PropertyInfo> electricitySupplyProperties;

        /// <summary>
        /// Словарь имен свойств типа <see cref="Model.ElectricitySupply"/>
        /// </summary>
        public static IEnumerable<string> ElectricitySupplyPropertiesNames
        {
            get
            {
                if (electricitySupplyPropertiesNames == null)
                {
                    electricitySupplyPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in ElectricitySupplyPropertiesCollection.Values)
                    {
                        ((IList)electricitySupplyPropertiesNames).Add(prop.Name);
                    }

                    (electricitySupplyPropertiesNames as List<string>).Sort();
                }

                return electricitySupplyPropertiesNames;
            }
        }

        /// <summary>
        /// Список свойств типа <see cref="Model.ElectricitySupply"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> ElectricitySupplyPropertiesCollection
        {
            get
            {
                if (electricitySupplyPropertiesCollection == null)
                {
                    lock (Lock)
                    {
                        Dictionary<string, PlusPropertyDescriptor> result = new();
                        PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(ElectricitySupplyType);

                        System.Diagnostics.Debug.Assert(collection.Count > 0, "TypeDescriptor.GetProperties(ElectricitySupplyType).Count <= 0");

                        IList<PlusPropertyDescriptor> plusPropsCollection = new List<PlusPropertyDescriptor>();
                        foreach (PropertyDescriptor pd in collection)
                        {
                            plusPropsCollection.Add(new PlusPropertyDescriptor(pd));
                        }

                        if (plusPropsCollection.All(i => i.Order == 0))
                        {
                            for (int ind = 0; ind < plusPropsCollection.Count; ind++)
                            {
                                plusPropsCollection[ind].Order = ind + 1;
                            }
                        }

                        SortedSet<PlusPropertyDescriptor> sortedCollection = new SortedSet<PlusPropertyDescriptor>(plusPropsCollection, new PropertyDescriptorComparer());

                        foreach (PlusPropertyDescriptor ppd in sortedCollection)
                        {
                            result.Add(ppd.Name, ppd);
                        }

                        electricitySupplyPropertiesCollection = result;
                    }
                }

                return electricitySupplyPropertiesCollection;
            }
        }

        public static Dictionary<string, PropertyInfo> ElectricitySupplyProperties
        {
            get
            {
                if (electricitySupplyProperties == null)
                {
                    electricitySupplyProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in ElectricitySupplyPropertiesNames)
                    {
                        electricitySupplyProperties.Add(prop, ElectricitySupplyType.GetProperty(prop));
                    }
                }

                return electricitySupplyProperties;
            }
        }

        public static IEnumerable<PlusPropertyDescriptor> GetElectricitySupplyPropertyDescriptors()
        {
            System.Collections.ObjectModel.ObservableCollection<string> electricitySupplyFields = new System.Collections.ObjectModel.ObservableCollection<string>(
                ElectricitySupplyPropertiesCollection.Values.Select(i => i.Name).ToList());

            IEnumerable<PlusPropertyDescriptor> fields = GetFields(ElectricitySupplyPropertiesCollection, electricitySupplyFields);
            int index = 0;
            foreach (PlusPropertyDescriptor item in fields)
            {
                item.Order = index++;
            }

            return fields.OrderBy(i => i.Order);
        }

        #endregion

        #region | Payment |

        public static Type PaymentType => typeof(Payment);

        private static IEnumerable<string> paymentPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> paymentPropertiesCollection;
        private static Dictionary<string, PropertyInfo> paymentProperties;

        /// <summary>
        /// Словарь имен свойств типа <see cref="Model.ElectricitySupply"/>
        /// </summary>
        public static IEnumerable<string> PaymentPropertiesNames
        {
            get
            {
                if (paymentPropertiesNames == null)
                {
                    paymentPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in PaymentPropertiesCollection.Values)
                    {
                        ((IList)paymentPropertiesNames).Add(prop.Name);
                    }

                    (paymentPropertiesNames as List<string>).Sort();
                }

                return paymentPropertiesNames;
            }
        }

        /// <summary>
        /// Список свойств типа <see cref="Model.Payment"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> PaymentPropertiesCollection
        {
            get
            {
                if (paymentPropertiesCollection == null)
                {
                    lock (Lock)
                    {
                        Dictionary<string, PlusPropertyDescriptor> result = new();
                        PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(PaymentType);

                        System.Diagnostics.Debug.Assert(collection.Count > 0, "TypeDescriptor.GetProperties(PaymentType).Count <= 0");

                        IList<PlusPropertyDescriptor> plusPropsCollection = new List<PlusPropertyDescriptor>();
                        foreach (PropertyDescriptor pd in collection)
                        {
                            plusPropsCollection.Add(new PlusPropertyDescriptor(pd));
                        }

                        if (plusPropsCollection.All(i => i.Order == 0))
                        {
                            for (int ind = 0; ind < plusPropsCollection.Count; ind++)
                            {
                                plusPropsCollection[ind].Order = ind + 1;
                            }
                        }

                        SortedSet<PlusPropertyDescriptor> sortedCollection = new SortedSet<PlusPropertyDescriptor>(plusPropsCollection, new PropertyDescriptorComparer());

                        foreach (PlusPropertyDescriptor ppd in sortedCollection)
                        {
                            result.Add(ppd.Name, ppd);
                        }

                        paymentPropertiesCollection = result;
                    }
                }

                return paymentPropertiesCollection;
            }
        }

        public static Dictionary<string, PropertyInfo> PaymentProperties
        {
            get
            {
                if (paymentProperties == null)
                {
                    paymentProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in PaymentPropertiesNames)
                    {
                        paymentProperties.Add(prop, PaymentType.GetProperty(prop));
                    }
                }

                return paymentProperties;
            }
        }

        public static IEnumerable<PlusPropertyDescriptor> GetPaymentPropertyDescriptors()
        {
            System.Collections.ObjectModel.ObservableCollection<string> paymentFields = new System.Collections.ObjectModel.ObservableCollection<string>(
                PaymentPropertiesCollection.Values.Select(i => i.Name).ToList());

            IEnumerable<PlusPropertyDescriptor> fields = GetFields(PaymentPropertiesCollection, paymentFields);
            int index = 0;
            foreach (PlusPropertyDescriptor item in fields)
            {
                item.Order = index++;
            }

            return fields.OrderBy(i => i.Order);
        }

        #endregion

        #region | SummaryInfoItem |

        private static readonly object Lock4 = new();

        public static Type SummaryInfoItemType => typeof(SummaryInfoItem);

        private static IEnumerable<string> summaryInfoItemPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> summaryInfoItemPropertiesCollection;
        private static Dictionary<string, PropertyInfo> summaryInfoItemProperties;

        public static ICollection<PlusPropertyDescriptor> SummaryInfoItemDescriptors => SummaryInfoItemPropertiesCollection?.Values;

        /// <summary>
        /// Словарь имен свойств типа <see cref="Model.ChangeOfMeter"/>
        /// </summary>
        public static IEnumerable<string> SummaryInfoItemPropertiesNames
        {
            get
            {
                if (summaryInfoItemPropertiesNames == null)
                {
                    summaryInfoItemPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in ChangesOfMetersPropertiesCollection.Values)
                    {
                        ((IList)summaryInfoItemPropertiesNames).Add(prop.Name);
                    }

                    (summaryInfoItemPropertiesNames as List<string>).Sort();
                }

                return summaryInfoItemPropertiesNames;
            }
        }

        /// <summary>
        /// Список свойств типа <see cref="Model.ChangeOfMeter"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> SummaryInfoItemPropertiesCollection
        {
            get
            {
                if (summaryInfoItemPropertiesCollection == null)
                {
                    lock (Lock4)
                    {
                        Dictionary<string, PlusPropertyDescriptor> result = new();
                        PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(SummaryInfoItemType);

                        System.Diagnostics.Debug.Assert(collection.Count > 0, "TypeDescriptor.GetProperties(SummaryInfoItemType).Count <= 0");

                        IList<PlusPropertyDescriptor> plusPropsCollection = new List<PlusPropertyDescriptor>();
                        foreach (PropertyDescriptor pd in collection)
                        {
                            plusPropsCollection.Add(new PlusPropertyDescriptor(pd));
                        }

                        if (plusPropsCollection.All(i => i.Order == 0))
                        {
                            for (int ind = 0; ind < plusPropsCollection.Count; ind++)
                            {
                                plusPropsCollection[ind].Order = ind + 1;
                            }
                        }

                        SortedSet<PlusPropertyDescriptor> sortedCollection = new SortedSet<PlusPropertyDescriptor>(plusPropsCollection, new PropertyDescriptorComparer());

                        foreach (PlusPropertyDescriptor ppd in sortedCollection)
                        {
                            result.Add(ppd.Name, ppd);
                        }

                        summaryInfoItemPropertiesCollection = result;
                    }
                }

                return summaryInfoItemPropertiesCollection;
            }
        }

        public static Dictionary<string, PropertyInfo> SummaryInfoItemProperties
        {
            get
            {
                if (summaryInfoItemProperties == null)
                {
                    summaryInfoItemProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in SummaryInfoItemPropertiesNames)
                    {
                        summaryInfoItemProperties.Add(prop, SummaryInfoItemType.GetProperty(prop));
                    }
                }

                return summaryInfoItemProperties;
            }
        }

        #endregion

        #region | For All Types

        public static object GetPropertyValue(IModel obj, string format, string property)
        {
            IDictionary<string, PropertyInfo> keyValuePairs = obj switch
            {
                Meter => MeterProperties,
                ChangeOfMeter => ChangesOfMetersProperties,
                ElectricitySupply => ElectricitySupplyProperties,
                _ => throw new NotImplementedException($"Unknown type '{obj.GetType()}'"),
            };

            return BaseGetPropertyValue(keyValuePairs, obj, format, property);
        }

        public static IEnumerable<PlusPropertyDescriptor> GetPropertyDescriptors(Type type)
        {
            IDictionary<string, PlusPropertyDescriptor> keyValuePairs = type switch
            {
                Type t when t == typeof(Meter) => MeterPropertiesCollection,
                Type t when t == typeof(ChangeOfMeter) => ChangesOfMetersPropertiesCollection,
                Type t when t == typeof(ElectricitySupply) => ElectricitySupplyPropertiesCollection,
                Type t when t == typeof(SummaryInfoItem) => null,
                _ => throw new NotImplementedException($"Unknown type '{type}'"),
            };

            return keyValuePairs?.Values;
        }

        public static PlusPropertyDescriptorsCollection GetMeterFieldsCollectionByTableViewKind(TableViewKinds tableViewKind)
        {
            return tableViewKind switch
            {
                TableViewKinds.BaseView => AppSettings.Default.ShortViewColumns,
                TableViewKinds.DetailedView => AppSettings.Default.DetailedViewColumns,
                TableViewKinds.ShortView => AppSettings.Default.ShortViewColumns,
                TableViewKinds.ОплатаView => AppSettings.Default.ОплатаViewColumns,
                TableViewKinds.ПривязкаView => AppSettings.Default.ПривязкаViewColumns,
                TableViewKinds.ПолныйView => AppSettings.Default.ПолныйViewColumns,
                _ => throw new NotImplementedException("Unknown TableView"),
            };
        }

        public static IEnumerable<PlusPropertyDescriptor> GetPropertyDescriptors(Type type, TableViewKinds tableViewKind)
        {
            IDictionary<string, PlusPropertyDescriptor> keyValuePairs = type switch
            {
                Type t when t == typeof(Meter) => MeterPropertiesCollection,
                Type t when t == typeof(ChangeOfMeter) => ChangesOfMetersPropertiesCollection,
                Type t when t == typeof(ElectricitySupply) => ElectricitySupplyPropertiesCollection,
                Type t when t == typeof(SummaryInfoItem) => null,
                _ => throw new NotImplementedException($"Unknown type '{type}'"),
            };
            MeterFieldsCollection fields = AppSettings.Default.GetMeterFieldsCollectionByTableViewKind(tableViewKind);

            return GetFields(keyValuePairs, fields);
        }

        #endregion

        public static IEnumerable<PlusPropertyDescriptor> GetFields(
            IDictionary<string, PlusPropertyDescriptor> keyValuePairs,
            ICollection propertyNames)
        {
            if (propertyNames == null)
            {
                return null;
            }

            if (keyValuePairs == null)
            {
                return null;
            }

            List<PlusPropertyDescriptor> result = new();
            foreach (object item in propertyNames)
            {
                if (item is PlusPropertyDescriptor descriptor)
                {
                    if (keyValuePairs.ContainsKey(descriptor.Name))
                    {
                        result.Add(keyValuePairs[descriptor.Name]);
                    }
                }
                else if (item is string name)
                {
                    if (keyValuePairs.ContainsKey(name))
                    {
                        result.Add(keyValuePairs[name]);
                    }
                }
            }

            return result;
        }

        private static object BaseGetPropertyValue(IDictionary<string, PropertyInfo> properties, object obj, string format, string property)
        {
            PropertyInfo pi = properties[property];
            object value = pi.GetValue(obj, null);

            if (value == null)
            {
                return string.Empty;
            }

            return pi.PropertyType switch
            {
                Type t when t == typeof(bool) => (bool)value ? "да" : "нет",
                Type t when t == typeof(Address) => ((Address)value).ToString(),
                Type t when t == typeof(TransformerSubstation) => ((TransformerSubstation)value).ToString(),

                _ => string.IsNullOrWhiteSpace(format) ? value : string.Format(format, value)
            };
        }

        internal class PropertyDescriptorComparer : IComparer<PlusPropertyDescriptor>
        {
            public int Compare(PlusPropertyDescriptor a, PlusPropertyDescriptor b)
            {
                if (a == null)
                {
                    return -1;
                }

                if (b == null)
                {
                    return 1;
                }

                int orderX = a.Order;
                int orderY = b.Order;

                if (a is IComparable ia)
                {
                    return ia.CompareTo(b);
                }
                else
                {
                    if (orderX == 0 && orderY == 0)
                    {
                        return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                    }
                    else
                    {
                        return orderX.CompareTo(orderY);
                    }
                }
            }
        }
    }
}
