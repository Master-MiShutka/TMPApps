using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;


using TMP.Shared;

namespace TMP.ARMTES.Model
{
    public class EditorModel : INotifyPropertyChanged
    {
        #region | Properties  |
        private string numberOfOrder;
        private string house;
        private string modemType;
        private string modemNetAddr;
        private string gsmNumber;
        private string city;
        private string street;
        #endregion
        public EditorModel()
        {
            ;
        }

        #region | Properties  |

        /*public bool HostAvailable
        {
            get { return _hostAvailable; }
            set
            {
                _hostAvailable = value;
                OnPropertyChanged("HostAvailable");
            }
        }*/

        #endregion

        #region | INotifyPropertyChanged Implementation |

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion | INotifyPropertyChanged |
    }

    public class RegistryCounter
    {
        public string Name { get; set; }
        public string NetworkAddress { get; set; }
        public string CounterType { get; set; }
        public string Number { get; set; }
        public byte TarifsCount { get; set; }
    }

    public class RegistryCollector
    {
        public string House { get; set; }
        public string ModemType { get; set; }
        public byte ModemNetAddr { get; set; }
        public string GsmNumber { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string CreationDate { get; set; }
        public string Description { get; set; }

        public uint NumberOfOrder { get; set; }
        public string Departament { get; set; }

        public List<RegistryCounter> Counters { get; set; }
        public RegistryCollector()
        {
            Counters = new List<RegistryCounter>();

            Description = Strings.ValueNotDefined;
        }

        public int CountersCount
        {
            get { return Counters == null ? 0 : Counters.Count; }
        }
    }

    public class RegistrySDSP : PropertyChangedBase
    {
        private List<string> _departaments;
        private List<RegistryCollector> _collectors;
        private string _filterProperty = string.Empty;
        private object _filterValue = null;
        public List<string> Departaments
        {
            get { return _departaments; }
            private set { SetProp<List<string>>(ref _departaments, value, "Departaments"); }
        }
        public List<RegistryCollector> Collectors
        {
            get
            {
                if (String.IsNullOrEmpty(_filterProperty))
                    return _collectors;
                else
                {
                    switch (_filterProperty)
                    {
                        case "Departament":
                            {
                                string filter = FilterValue.ToString();
                                List<RegistryCollector> result = _collectors.Where((c, i) => c.Departament == filter).ToList<RegistryCollector>();
                                if (result.Count == 0)
                                    return _collectors;
                                else
                                    return result;
                            }
                        default:
                            return null;
                    }
                }
            }
            private set { SetProp<List<RegistryCollector>>(ref _collectors, value, "Collectors"); }
        }

        public string FilterProperty
        {
            get { return _filterProperty; }
            set 
            {
                SetProp<string>(ref _filterProperty, value, "FilterProperty");
                base.RaisePropertyChanged("Collectors");
                base.RaisePropertyChanged("CollectorsCount");
                base.RaisePropertyChanged("CountersCount");
            }
        }

        public object FilterValue
        {
            get { return _filterValue; }
            set { SetProp<object>(ref _filterValue, value, "FilterValue"); }
        }

        public int DepartamentsCount
        {
            get { return Departaments == null ? 0 : Departaments.Count; }
        }

        public int CollectorsCount
        {
            get { return Collectors == null ? 0 : Collectors.Count; }
        }

        public int CountersCount
        {
            get
            { 
                if (Collectors == null) return 0;
                else
                {
                    int count = 0;
                    for (int i = 0; i < Collectors.Count; i++)
                    {
                        if (Collectors[i].Counters == null)
                        {
                            continue;
                        }
                        else
                            count += Collectors[i].Counters.Count;
                    }
                    return count;
                }
            }
        }

        public RegistrySDSP()
        {
            Departaments = new List<string>();
            Collectors = new List<RegistryCollector>();
        }

        public void Initialize(List<string> departaments, List<RegistryCollector> collectors)
        {
            Departaments = departaments;
            Collectors = collectors;

            FilterValue = null;
            FilterProperty = string.Empty;
        }

        public void Update()
        {
            base.RaisePropertyChanged("FilterProperty");
            base.RaisePropertyChanged("Collectors");
            base.RaisePropertyChanged("CollectorsCount");
            base.RaisePropertyChanged("CountersCount");
        }
    }
}
