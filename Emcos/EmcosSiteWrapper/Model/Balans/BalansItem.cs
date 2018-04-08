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
    public abstract class BalansItem : PropertyChangedBase, IBalansItem
    {
        #region Fields

        private double? _addToEminus;
        private double? _addToEplus;
        private string _code;
        private string _description;

        private IList<double?> _dailyEplus;
        private IList<double?> _dailyEminus;

        private double? _monthEPlus, _monthEMinus;

        private int _id;
        private DataStatus _status = DataStatus.Wait;
        private string _name;

        private bool _useMonthValue = false;

        #region Constructor

        public BalansItem()
        {
            UseMonthValue = false;
        }

        #endregion

        #region Common Properties

        [DataMember(IsRequired = true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; RaisePropertyChanged("Id"); }
        }

        [DataMember(IsRequired = true)]
        public string Name { get { return _name; } set { _name = value; RaisePropertyChanged("Name"); } }


        public virtual ElementTypes ElementType { get; }
        public string TypeName
        {
            get
            {
                var fieldInfo = ElementType.GetType().GetField(ElementType.ToString());
                var attribArray = fieldInfo.GetCustomAttributes(false);
                var attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }

        [IgnoreDataMember]
        public string TypeCode { get; set; }

        [IgnoreDataMember]
        public string EcpName { get; set; }

        [DataMember(IsRequired = true)]
        public string Code
        {
            get { return _code; }
            set { _code = value; RaisePropertyChanged("Code"); }
        }

        [DataMember]
        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged("Description"); }
        }

        [DataMember()]
        public ObservableCollection<IBalansItem> Children { get; set; }

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

        #endregion

        #region E+

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

        #endregion

        #region E-

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

        #endregion










        public string Correction
        {
            get
            {
                var s1 = String.Empty;
                var s2 = String.Empty;
                if (AddToEplus.HasValue)
                {
                    s1 = AddToEplus.Value < 0 ? String.Format("E+ {0:N2}", AddToEplus) : String.Format("E+ +{0:N2}", AddToEplus);
                }
                if ( AddToEminus.HasValue)
                {
                    s2 = AddToEminus.Value < 0 ? String.Format("E- {0:N2}", AddToEminus) : String.Format("E- +{0:N2}", AddToEminus);
                }                
                if (String.IsNullOrEmpty(s1) == false && String.IsNullOrEmpty(s2) == false)
                    return String.Format("{0}\t{1}", s1, s2);
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



        public double? Eminus
        {
            get { return UseMonthValue ? MonthEminus : SummOfDaysEminusValue; }
            set
            {
                SummOfDaysEminusValue = value;
                RaisePropertyChanged("Eminus");
                RaisePropertyChanged("EnergyOut");
            }
        }

        public double? EnergyIn
        {
            get { return AddToEplus.HasValue ? (Eplus == null ? 0.0 : Eplus) + AddToEplus : Eplus; }
        }

        public double? EnergyOut
        {
            get { return AddToEminus.HasValue ? (Eminus == null ? 0.0 : Eminus) + AddToEminus : Eminus; }
        }

        [DataMember]
        public double? Eplus
        {
            get { return UseMonthValue ? MonthEplus : SummOfDaysEplusValue; }
            set
            {
                SummOfDaysEplusValue = value;
                RaisePropertyChanged("Eplus");
                RaisePropertyChanged("EnergyIn");
            }
        }

        public double? FideraIn { get { return null; } }

        public double? FideraOut { get { return null; } }



        [DataMember()]
        public double? MonthEminus
        {
            get { return _monthEMinus; }
            set
            {
                _monthEMinus = value;
                RaisePropertyChanged("MonthEminus");
                RaisePropertyChanged("Eminus");
                RaisePropertyChanged("EnergyOut");
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
                RaisePropertyChanged("Eminus");
                RaisePropertyChanged("EnergyOut");
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



        public Substation Substation { get; private set; }
        public void SetSubstation(Substation substation)
        {
            Substation = substation;
        }



        public double? Tsn { get { return null; } }
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
            SummOfDaysEminusValue = null;
            SummOfDaysEplusValue = null;
            GC.Collect();
            /*RaisePropertyChanged("EnergyIn");
            RaisePropertyChanged("EnergyOut");*/
        }


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



        [DataMember()]
        public double? SummOfDaysEminusValue { get; set; }
        [DataMember()]
        public double? SummOfDaysEplusValue { get; set; }

        private void RaisePropertiesChanged()
        {
            RaisePropertyChanged("EnergyIn");
            RaisePropertyChanged("EnergyOut");
        }
    }
}
