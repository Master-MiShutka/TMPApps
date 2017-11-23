using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace TMP.WORK.AramisChetchiki
{
    public static class ModelHelper
    {
        #region | Meter |

        private static ICollection<string> _meterPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> _meterPropertiesCollection;
        private static Dictionary<string, PropertyInfo> _meterProperties;
        private static Dictionary<string, string> _meterPropertyDisplayNames;
        private static IList<string> _meterSummaryInfoProperties;
        private static IDictionary<string, PlusPropertyDescriptor> _meterSummaryInfoPropertiesCollection;

        public static Type MeterType => typeof(Model.Meter);

        /// <summary>
        /// Список имен свойств типа <see cref="Model.Meter"/>
        /// </summary>
        public static ICollection<string> MeterPropertiesNames
        {
            get
            {
                if (_meterPropertiesNames == null)
                {
                    _meterPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in MeterPropertiesCollection.Values)
                    {
                        _meterPropertiesNames.Add(prop.Name);
                    }
                    (_meterPropertiesNames as List<string>).Sort();
                }
                return _meterPropertiesNames;
            }
        }

        /// <summary>
        /// Словарь свойств типа <see cref="Model.Meter"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> MeterPropertiesCollection
        {
            get
            {
                if (_meterPropertiesCollection == null)
                {
                    _meterPropertiesCollection = new Dictionary<string, PlusPropertyDescriptor>();
                    var collection = TypeDescriptor.GetProperties(MeterType);
                    if (_meterPropertiesCollection == null)
                        throw new ArgumentNullException();

                    var sortedCollection = new SortedSet<PlusPropertyDescriptor>(new PropertyDescriptorComparer());
                    foreach (PropertyDescriptor pd in collection)
                        sortedCollection.Add(new PlusPropertyDescriptor(pd));

                    foreach (PlusPropertyDescriptor ppd in sortedCollection)
                        _meterPropertiesCollection.Add(ppd.Name, ppd);
                }
                return _meterPropertiesCollection;
            }
        }


        public static Dictionary<string, PropertyInfo> MeterProperties
        {
            get
            {
                if (_meterProperties == null)
                {
                    _meterProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in MeterPropertiesNames)
                        _meterProperties.Add(prop, MeterType.GetProperty(prop));
                }
                return _meterProperties;
            }
        }

        public static Dictionary<string, string> MeterPropertyDisplayNames
        {
            get
            {
                if (_meterPropertyDisplayNames == null)
                {
                    _meterPropertyDisplayNames = new Dictionary<string, string>();
                    foreach (PlusPropertyDescriptor prop in MeterPropertiesCollection.Values)
                        _meterPropertyDisplayNames.Add(prop.Name, prop.DisplayName);
                }
                return _meterPropertyDisplayNames;
            }
        }

        public static IList<string> MeterSummaryInfoProperties
        {
            get
            {
                if (_meterSummaryInfoProperties == null)
                {
                    _meterSummaryInfoProperties = new List<string>();
                    // атрибут, показывающий о необходимости сбора сводной информации
                    Type summaryInfoAttr = typeof(Model.SummaryInfoAttribute);
                    foreach (PlusPropertyDescriptor pd in MeterPropertiesCollection.Values)
                        if (pd.Attributes[summaryInfoAttr] != null)
                            _meterSummaryInfoProperties.Add(pd.Name);
                }
                return _meterSummaryInfoProperties;
            }
        }

        public static IDictionary<string, PlusPropertyDescriptor> MeterSummaryInfoPropertiesCollection
        {
            get
            {
                if (_meterSummaryInfoPropertiesCollection == null)
                {
                    _meterSummaryInfoPropertiesCollection = new Dictionary<string, PlusPropertyDescriptor>();
                    PropertyDescriptorCollection collection = TypeDescriptor.GetProperties(MeterType);
                    if (_meterSummaryInfoPropertiesCollection == null)
                        throw new ArgumentNullException();

                    // атрибут, показывающий о необходимости сбора сводной информации
                    Type summaryInfoAttr = typeof(Model.SummaryInfoAttribute);

                    IList<PlusPropertyDescriptor> plusPropsCollection = new List<PlusPropertyDescriptor>();
                    foreach (PropertyDescriptor pd in collection)
                        if (pd.Attributes[summaryInfoAttr] != null)
                            plusPropsCollection.Add(new PlusPropertyDescriptor(pd));
                    if (plusPropsCollection.All(i => i.Order == 0))
                    {
                        for (int ind = 0; ind < plusPropsCollection.Count; ind++)
                            plusPropsCollection[ind].Order = ind + 1;
                    }

                    var sortedCollection = new SortedSet<PlusPropertyDescriptor>(plusPropsCollection, new PropertyDescriptorComparer());
                    
                    foreach (PlusPropertyDescriptor ppd in sortedCollection)
                        _meterSummaryInfoPropertiesCollection.Add(ppd.Name, ppd);
                }
                return _meterSummaryInfoPropertiesCollection;
            }
        }

        public static object MeterGetPropertyValue(Model.Meter meter, string property)
        {
            PropertyInfo pi = MeterProperties[property];
            object value = pi.GetValue(meter, null);
            if (pi.PropertyType == typeof(bool))
            {
                return (bool)value ? "да" : "нет";
            }
            return value;
        }

        #endregion

        #region | ChangesOfMeters |

        public static Type ChangesOfMeterType => typeof(Model.ChangesOfMeters);

        private static IEnumerable<string> _changesOfMeterPropertiesNames;
        private static IDictionary<string, PlusPropertyDescriptor> _changesOfMeterPropertiesCollection;
        private static Dictionary<string, PropertyInfo> _changesOfMeterProperties;


        /// <summary>
        /// Словарь имен свойств типа <see cref="Model.ChangesOfMeters"/>
        /// </summary>
        public static IEnumerable<string> ChangesOfMeterPropertiesNames
        {
            get
            {
                if (_changesOfMeterPropertiesNames == null)
                {
                    _changesOfMeterPropertiesNames = new List<string>();
                    foreach (PlusPropertyDescriptor prop in ChangesOfMetersPropertiesCollection.Values)
                    {
                        ((IList)_changesOfMeterPropertiesNames).Add(prop.Name);
                    }
                    (_changesOfMeterPropertiesNames as List<string>).Sort();
                }
                return _changesOfMeterPropertiesNames;
            }
        }

        /// <summary>
        /// Список свойств типа <see cref="Model.ChangesOfMeters"/>
        /// </summary>
        public static IDictionary<string, PlusPropertyDescriptor> ChangesOfMetersPropertiesCollection
        {
            get
            {
                if (_changesOfMeterPropertiesCollection == null)
                {
                    _changesOfMeterPropertiesCollection = new Dictionary<string, PlusPropertyDescriptor>();
                    var collection = TypeDescriptor.GetProperties(ChangesOfMeterType);
                    if (_changesOfMeterPropertiesCollection == null)
                        throw new ArgumentNullException();

                    IList<PlusPropertyDescriptor> plusPropsCollection = new List<PlusPropertyDescriptor>();
                    foreach (PropertyDescriptor pd in collection)
                        plusPropsCollection.Add(new PlusPropertyDescriptor(pd));
                    if (plusPropsCollection.All(i => i.Order == 0))
                    {
                        for (int ind = 0; ind < plusPropsCollection.Count; ind++)
                            plusPropsCollection[ind].Order = ind + 1;
                    }

                    var sortedCollection = new SortedSet<PlusPropertyDescriptor>(plusPropsCollection, new PropertyDescriptorComparer());

                    foreach (PlusPropertyDescriptor ppd in sortedCollection)
                        _changesOfMeterPropertiesCollection.Add(ppd.Name, ppd);
                }
                return _changesOfMeterPropertiesCollection;
            }
        }

        public static Dictionary<string, PropertyInfo> ChangesOfMetersProperties
        {
            get
            {
                if (_changesOfMeterProperties == null)
                {
                    _changesOfMeterProperties = new Dictionary<string, PropertyInfo>();
                    foreach (string prop in ChangesOfMeterPropertiesNames)
                        _changesOfMeterProperties.Add(prop, ChangesOfMeterType.GetProperty(prop));
                }
                return _changesOfMeterProperties;
            }
        }

        public static object ChangesOfMetersGetPropertyValue(Model.ChangesOfMeters change, string format, string property)
        {
            PropertyInfo pi = ChangesOfMetersProperties[property];
            object value = pi.GetValue(change, null);
            if (pi.PropertyType == typeof(bool))
            {
                return (bool)value ? "да" : "нет";
            }
            if (String.IsNullOrWhiteSpace(format))
                return value;
            else
                return string.Format("{0:" + format + "}", value);
        }

        #endregion

        internal class PropertyDescriptorComparer : IComparer<PlusPropertyDescriptor>
        {
            public int Compare(PlusPropertyDescriptor a, PlusPropertyDescriptor b)
            {
                if (a == null) return -1;
                if (b == null) return 1;

                int oderX = a.Order;
                int oderY = b.Order;

                IComparable ia = a as IComparable;
                if (ia != null)
                    return ia.CompareTo(b);
                else
                    return -1;
            }
        }
    }

    public class PlusPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor _innerPropertyDescriptor;
        private bool _readOnly;
        private System.ComponentModel.DataAnnotations.DisplayAttribute _displayAttribute;

        public PlusPropertyDescriptor(PropertyDescriptor inner) : base(inner)
        {
            _innerPropertyDescriptor = inner;
            _readOnly = inner.IsReadOnly;
            _displayAttribute = Attributes[typeof(System.ComponentModel.DataAnnotations.DisplayAttribute)] as System.ComponentModel.DataAnnotations.DisplayAttribute;

            if (_displayAttribute != null)
                Order = _displayAttribute.Order;
        }

        public override Type ComponentType => _innerPropertyDescriptor.GetType();

        public override bool IsReadOnly => false;

        public override Type PropertyType => _innerPropertyDescriptor.PropertyType;

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return _innerPropertyDescriptor.GetValue(component);
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            _innerPropertyDescriptor = (PlusPropertyDescriptor)value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public void SetOrder(int order)
        {
            Order = order;
        }

        public string GroupName => _displayAttribute?.GroupName;
        public int Order { get; set; }
        public bool IsVisible => base.IsBrowsable;
    }
}
