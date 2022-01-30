namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using TMP.Shared;
    using TMP.WORK.AramisChetchiki.Model;

    public static class ModelHelper
    {
        public static event EventHandler Initialized;

        public static void Init()
        {
            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() =>
            {
                InitMode();

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

                IEnumerable<string> list13 = SummaryInfoItemPropertiesNames;
                IDictionary<string, PlusPropertyDescriptor> list14 = SummaryInfoItemPropertiesCollection;
                Dictionary<string, PropertyInfo> list15 = SummaryInfoItemProperties;
            });
        }

        #region | Mode |

        public static readonly IDictionary<Model.Mode, string> ModesDescription = new Dictionary<Model.Mode, string>();
        public static readonly IDictionary<Model.Mode, (Type view, Type vm)> ModesViewModelTypes = new Dictionary<Model.Mode, (Type view, Type vm)>();
        public static readonly IDictionary<Type, Model.Mode> ViewModelToModeDictionary = new Dictionary<Type, Model.Mode>();

        private static void InitMode()
        {
            ModesDescription.Clear();
            ModesViewModelTypes.Clear();
            ViewModelToModeDictionary.Clear();

            foreach (Model.Mode mode in Enum.GetValues(typeof(Model.Mode)))
            {
                System.Reflection.FieldInfo info = mode.GetType().GetField(mode.ToString());
                DescriptionAttribute[] valueDescription = (System.ComponentModel.DescriptionAttribute[])info.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

                Type viewType = null, viewModelType = null;

                object[] viewAttr = info.GetCustomAttributes(typeof(Shared.ViewAttribute), false);
                if (viewAttr != null && viewAttr.Length == 1)
                {
                    viewType = ((Shared.ViewAttribute)viewAttr[0]).ViewType;
                }

                object[] viewModelAttr = info.GetCustomAttributes(typeof(Shared.ViewModelAttribute), false);
                if (viewModelAttr != null && viewModelAttr.Length == 1 && viewModelAttr[0] is Shared.ViewModelAttribute viewModelAttribute)
                {
                    viewModelType = viewModelAttribute.ViewModelType;
                }

                if (valueDescription?.Length == 1 && /*viewAttr?.Length > 0 &&*/ viewModelAttr?.Length > 0 && viewModelType != null)
                {
                    ModesDescription.Add(mode, valueDescription[0].Description);

                    ModesViewModelTypes.Add(mode, (viewType, viewModelType));

                    ViewModelToModeDictionary.Add(viewModelType, mode);
                }
            }

            Initialized?.Invoke(null, EventArgs.Empty);
        }

        public static ViewModel.IViewModel ModeFactory(Model.Mode mode)
        {
            ViewModel.IViewModel viewModel = default;
            Type viewType = null, viewModelType = null;

            if (ModesViewModelTypes.ContainsKey(mode))
            {
                (viewType, viewModelType) = ModesViewModelTypes[mode];

                if (viewModelType.IsGenericType)
                {
                    Type[] typeArgs = viewModelType.GetGenericArguments();
                    Type constructed = viewType.MakeGenericType(typeArgs);
                    viewModel = (ViewModel.IViewModel)Activator.CreateInstance(constructed);
                }
                else
                {
                    viewModel = viewModelType != typeof(ViewModel.ViewCollectionViewModel)
                        ? (ViewModel.IViewModel)Activator.CreateInstance(viewModelType)
                        : (ViewModel.IViewModel)Activator.CreateInstance(
                            viewModelType,
                            BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                            null,
                            new object[] { Type.Missing },
                            AppSettings.CurrentCulture);
                }
            }

            return viewModel;
        }

        #endregion

        #region | Meter |

        private static IList<string> meterPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> meterPropertiesCollection;
        private static Dictionary<string, PropertyInfo> meterProperties;
        private static Dictionary<string, string> meterPropertyDisplayNames;
        private static IList<string> meterSummaryInfoProperties;
        private static IDictionary<string, PlusPropertyDescriptor> meterSummaryInfoPropertiesCollection;

        public static Type MeterType => typeof(Model.Meter);

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
                    Type summaryInfoAttr = typeof(Model.SummaryInfoAttribute);
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
                    Type summaryInfoAttr = typeof(Model.SummaryInfoAttribute);

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

        public static object MeterGetPropertyValue(Model.Meter meter, string property)
        {
            PropertyInfo pi = MeterProperties[property];
            object value = pi.GetValue(meter, null);
            if (pi.PropertyType == typeof(bool))
            {
                return (bool)value ? "да" : "нет";
            }

            if (pi.PropertyType == typeof(Model.Address))
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

        #region | ChangesOfMeters |

        private static readonly object Lock = new();

        public static Type ChangesOfMeterType => typeof(Model.ChangeOfMeter);

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

        #endregion

        #region | ElectricitySupply |

        public static Type ElectricitySupplyType => typeof(Model.ElectricitySupply);

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

        #endregion

        #region | SummaryInfoItem |

        private static readonly object Lock4 = new();

        public static Type SummaryInfoItemType => typeof(Model.SummaryInfoItem);

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
                if (item is Shared.PlusPropertyDescriptor descriptor)
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
                Type t when t == typeof(Model.Address) => ((Model.Address)value).ToString(),
                Type t when t == typeof(Model.TransformerSubstation) => ((Model.TransformerSubstation)value).ToString(),

                _ => string.IsNullOrWhiteSpace(format) ? value : string.Format(AppSettings.CurrentCulture, format, value)
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
