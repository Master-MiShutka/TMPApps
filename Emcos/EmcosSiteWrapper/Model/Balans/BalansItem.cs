using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "BalansItem")]
    public class BalansItem : PropertyChangedBase, IBalansItem, IDisposable
    {
        private double? _addToEminus;
        private double? _addToEplus;
        private string _code;
        private string _description;

        private IList<double?> _dailyEplus;
        private IList<double?> _dailyEminus;

        private double? _monthEPlus, _monthEMinus;

        private decimal _id;
        private DataStatus _status = DataStatus.Wait;
        private string _name;
        public BalansItem()
        {
            UseMonthValue = false;
        }
        ~BalansItem()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            ;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        [DataMember()]
        public double? AddToEminus
        {
            get { return _addToEminus; }
            set
            {
                _addToEminus = value;
                RaisePropertyChanged("AddToEminus");
                RaisePropertyChanged("Eminus");
                RaisePropertyChanged("EnergyOut");
                RaisePropertyChanged("Correction");
            }
        }

        [DataMember()]
        public double? AddToEplus
        {
            get { return _addToEplus; }
            set
            {
                _addToEplus = value;
                RaisePropertyChanged("AddToEplus");
                RaisePropertyChanged("Eplus");
                RaisePropertyChanged("EnergyIn");
                RaisePropertyChanged("Correction");
            }
        }

        [DataMember()]
        public ObservableCollection<IBalansItem> Children { get; set; }

        [DataMember(IsRequired = true)]
        public string Code
        {
            get { return _code; }
            set { _code = value; RaisePropertyChanged("Code"); }
        }
        public string Correction
        {
            get
            {
                var s1 = String.Empty;
                var s2 = String.Empty;
                if (AddToEplus != null && AddToEplus.HasValue)
                {
                    s1 = AddToEplus.Value < 0 ? String.Format("E+ {0:N2}", AddToEplus) : String.Format("E+ +{0:N2}", AddToEplus);
                }
                if (AddToEminus != null && AddToEminus.HasValue)
                {
                    s2 = AddToEminus.Value < 0 ? String.Format("E- {0:N2}", AddToEminus) : String.Format("E- +{0:N2}", AddToEminus);
                }                
                if (String.IsNullOrEmpty(s1) == false && String.IsNullOrEmpty(s2) == false)
                    String.Format("{0}\n{1}", s1, s2);
                if (String.IsNullOrEmpty(s1) == true && String.IsNullOrEmpty(s2) == true)
                    return String.Empty;
                if (String.IsNullOrEmpty(s1) == false && String.IsNullOrEmpty(s2) == true)
                    return s1;
                if (String.IsNullOrEmpty(s1) == true && String.IsNullOrEmpty(s2) == false)
                    return s2;
                return null;
            }
        }
        [DataMember()]
        [Newtonsoft.Json.JsonConverter(typeof(NullListToEmptyStringConverter))]
        public IList<double?> DailyEminus
        {
            get { return _dailyEminus; }
            set
            {
                _dailyEminus = value;
                RaisePropertyChanged("DailyEminus");
                RaisePropertyChanged("DataMinusStatus");
                RaisePropertyChanged("NotFullDataMinus");
                RaisePropertyChanged("NotFullData");

                RaisePropertyChanged("DailyEminusValues");
                RaisePropertyChanged("DailyEminusValuesAverage");
                RaisePropertyChanged("DailyEminusValuesMax");
                RaisePropertyChanged("DailyEminusValuesMin");
                RaisePropertyChanged("DailyEminusValuesSum");
            }
        }

        [DataMember()]
        [Newtonsoft.Json.JsonConverter(typeof(NullListToEmptyStringConverter))]
        public IList<double?> DailyEplus
        {
            get { return _dailyEplus; }
            set
            {
                _dailyEplus = value;
                RaisePropertyChanged("DailyEplus");
                RaisePropertyChanged("DataPlusStatus");
                RaisePropertyChanged("NotFullDataPlus");
                RaisePropertyChanged("NotFullData");

                RaisePropertyChanged("DailyEplusValues");
                RaisePropertyChanged("DailyEplusValuesAverage");
                RaisePropertyChanged("DailyEplusValuesMax");
                RaisePropertyChanged("DailyEplusValuesMin");
                RaisePropertyChanged("DailyEplusValuesSum");
            }
        }

        public string DataMinusStatus
        {
            get
            {
                if (DailyEminus == null || DailyEminus.Count == 0)
                    return null;
                int missingDataMinusCount = DailyEminus.Where((d) => d.HasValue == false).Count();
                return missingDataMinusCount == 0 ? null : String.Format("{0} из {1}", missingDataMinusCount, DailyEminus.Count);
            }
        }

        public string DataPlusStatus
        {
            get
            {
                if (DailyEplus == null || DailyEplus.Count == 0)
                    return null;
                int missingDataPlusCount = DailyEplus.Where((d) => d.HasValue == false).Count();
                return missingDataPlusCount == 0 ? null : String.Format("{0} из {1}", missingDataPlusCount, DailyEplus.Count);
            }
        }

        [DataMember]
        public string Description {
            get { return _description; }
            set { _description = value; RaisePropertyChanged("Description"); } }

        [DataMember]
        public double? Eminus
        {
            get { return UseMonthValue ? MonthEminus : DayEminusValue; }
            set
            {
                DayEminusValue = value;
                RaisePropertyChanged("Eminus");
                RaisePropertyChanged("EnergyOut");
            }
        }

        public double? EnergyIn
        {
            get { return (AddToEplus != null && AddToEplus.HasValue) ? (Eplus == null ? 0.0 : Eplus) + AddToEplus : Eplus; }
        }

        public double? EnergyOut
        {
            get { return (AddToEminus != null && AddToEminus.HasValue) ? (Eminus == null ? 0.0 : Eminus) + AddToEminus : Eminus; }
        }

        [DataMember]
        public double? Eplus
        {
            get { return UseMonthValue ? MonthEplus : DayEplusValue; }
            set
            {
                DayEplusValue = value;
                RaisePropertyChanged("Eplus");
                RaisePropertyChanged("EnergyIn");
            }
        }

        public double? FideraIn { get { return null; } }

        public double? FideraOut { get { return null; } }

        [DataMember(IsRequired = true)]
        public decimal Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                RaisePropertyChanged("Id"); 
            }
        }

        [DataMember()]
        public double? MonthEminus
        {
            get { return _monthEMinus; }
            set
            {
                _monthEMinus = value;
                RaisePropertyChanged("MonthEminus");
            }
        }

        [DataMember()]
        public double? MonthEplus
        {
            get { return _monthEPlus; }
            set
            {
                _monthEPlus = value;
                RaisePropertyChanged("MonthEplus");
            }
        }

        public bool NotFullData { get { return NotFullDataPlus || NotFullDataMinus; } }

        public bool NotFullDataMinus
        {
            get
            {
                return (DailyEminus != null && DailyEminus.Count > 0)
                    ? DailyEminus.Any((item) => item.HasValue == false)
                    : false;
            }
        }

        public bool NotFullDataPlus
        {
            get
            {
                return (DailyEplus != null && DailyEplus.Count > 0)
                    ? DailyEplus.Any((item) => item.HasValue == false)
                    : false;
            }
        }

        public double? PercentageOfUnbalance { get { return null; } }

        [DataMember(IsRequired = true)]
        public DataStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        public Substation Substation { get; private set; }
        public void SetSubstation(Substation substation)
        {
            Substation = substation;
        }

        [DataMember(IsRequired = true)]
        public string Name { get { return _name; } set { _name = value; RaisePropertyChanged("Name"); } }
        public double? Tsn { get { return null; } }
        public virtual ElementTypes Type { get; set ; }
        public string TypeName
        {
            get
            {
                var fieldInfo = Type.GetType().GetField(Type.ToString());
                var attribArray = fieldInfo.GetCustomAttributes(false);
                var attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }
        public double? Unbalance { get { return null; } }
        public double? VvodaIn { get { return null; } }
        public double? VvodaOut { get { return null; } }

        public void Clear()
        {
            _addToEminus = null;
            _addToEplus = null;
            DailyEminus = null;
            DailyEplus = null;
            MonthEminus = null;
            MonthEplus = null;
            UseMonthValue = false;
            DayEminusValue = null;
            DayEplusValue = null;
            GC.Collect();
            /*RaisePropertyChanged("EnergyIn");
            RaisePropertyChanged("EnergyOut");*/
        }

        #region VALUES
        public double? DifferenceBetweenDailySumAndMonthPlus
        {
            get { return MonthEplus - Eplus; }
        }
        public double? DifferenceBetweenDailySumAndMonthMinus
        {
            get { return MonthEminus - Eminus; }
        }
        public IList<Value> DailyEminusValues
        {
            get
            {
                if (UseMonthValue) return null;
                if (DailyEminus == null) return null;
                IList<Value> result = new List<Value>();
                var max = DailyEminus.Max();
                foreach (double? item in DailyEminus)
                    result.Add((max == null || item == null)
                        ? new Value
                        { DoubleValue = 0, PercentValue = 0, Status = ValueStatus.Missing }
                        : new Value
                        { DoubleValue = item.Value, PercentValue = item.Value / max, Status = ValueStatus.Normal });
                return result;
            }
        }

        public double? DailyEminusValuesAverage
        {
            get
            {
                double? value;
                if (DailyEminusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEminusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = values.Average((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }

        public double? DailyEminusValuesMax
        {
            get
            {
                double? value;
                if (DailyEminusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEminusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = values.Max((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }

        public double? DailyEminusValuesMin
        {
            get
            {
                double? value;
                if (DailyEminusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEminusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = values.Min((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }

        public double? DailyEminusValuesSum
        {
            get
            {
                double? value;
                if (DailyEminusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEminusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = values.Sum((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }

        public IList<Value> DailyEplusValues
        {
            get
            {
                if (UseMonthValue) return null;
                if (DailyEplus == null) return null;
                IList<Value> result = new List<Value>();
                var max = DailyEplus.Max();
                foreach (double? item in DailyEplus)
                    result.Add((max == null || item == null)
                        ? new Value
                        { DoubleValue = 0, PercentValue = 0, Status = ValueStatus.Missing }
                        : new Value
                        { DoubleValue = item.Value, PercentValue = item.Value / max, Status = ValueStatus.Normal });
                return result;
            }
        }
        public double? DailyEplusValuesAverage
        {
            get
            {
                double? value;
                if (DailyEplusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEplusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = values.Average((i) =>i.DoubleValue.Value);
                }
                return value;
            }
        }

        public double? DailyEplusValuesMax
        {
            get
            {
                double? value;
                if (DailyEplusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEplusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = DailyEplusValues.Max((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }

        public double? DailyEplusValuesMin
        {
            get
            {
                double? value;
                if (DailyEplusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEplusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = values.Min((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }

        public double? DailyEplusValuesSum
        {
            get
            {
                double? value;
                if (DailyEplusValues == null)
                    return null;
                else
                {
                    IList<Value> values = DailyEplusValues.Where(i => i.Status != ValueStatus.Missing).ToList();
                    if (values == null || values.Count == 0) return null;
                    value = DailyEplusValues.Sum((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }
        #endregion

        [OnDeserializing()]
        private void OnDeserializingMethod(StreamingContext context)
        {            
            ;
        }

        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context)
        {
            ;
        }
        public virtual IBalansItem Copy()
        {
            throw new NotImplementedException();
        }
        private bool _useMonthValue = false;
        [DataMember()]
        public bool UseMonthValue
        {
            get { return _useMonthValue; }
            set
            {
                _useMonthValue = value;
                RaisePropertiesChanged();
            }
        }
        [DataMember()]
        public double? DayEminusValue { get; set; }
        [DataMember()]
        public double? DayEplusValue { get; set; }

        private void RaisePropertiesChanged()
        {
            RaisePropertyChanged("EnergyIn");
            RaisePropertyChanged("EnergyOut");
        }
    }
}
