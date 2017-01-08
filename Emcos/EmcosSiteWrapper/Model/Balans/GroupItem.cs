using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "GroupItem")]
    public class GroupItem : PropertyChangedBase, IBalansGroup, IProgress, IDisposable
    {
        private int _id;
        private string _code;
        private string _name;

        private string _description;
        private double? _maximumAllowableUnbalance = null;

        public GroupItem()
        {
            Children = new ObservableCollection<IBalansItem>();
        }
        ~GroupItem()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (Items != null)
                foreach (IBalansItem item in Items)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                    item.Dispose();
                }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DataMember()]
        public int Id { get { return _id; } set { _id = value; RaisePropertyChanged("Id"); } }

        [DataMember()]
        public string Code { get { return _code; } set { _code = value; RaisePropertyChanged("Code"); } }
        public string Correction
        {
            get
            {
                if (Items == null) return null;
                var sb = new System.Text.StringBuilder();
                foreach (IBalansItem item in Items)
                {
                    if ((item.AddToEplus != null && item.AddToEplus.HasValue) || (item.AddToEminus != null && item.AddToEminus.HasValue))
                    {
                        sb.AppendLine(item.Name);
                        sb.AppendLine(item.Correction);
                    }
                }
                if (sb.Length == 0)
                    return null;
                else
                    return sb.ToString().Trim();
            }
        }
        [DataMember()]
        public string Name { get { return _name; } set { _name = value; RaisePropertyChanged("Name"); } }
        [JsonIgnore]
        public virtual double? Eplus
        {
            get
            {
                var result = new Nullable<double>(0);
                if (Items != null)
                    foreach (IBalansItem item in Items)
                    {
                        var value = item.Eplus;
                        if (value.HasValue)
                            result += value;
                    }
                return result == 0 ? null : result;
            }

            set { throw new InvalidOperationException("Свойство Eplus только для чтения. Тип: " + GetType() + ", элемент: " + Name + ", значение: " + value); }
        }
        [JsonIgnore]
        public double? AddToEplus
        {
            get
            {
                if (Items == null) return null;
                double? sum = Items.Sum((i) => (i.AddToEplus != null && i.AddToEplus.HasValue) ? i.AddToEplus.Value : 0.0);
                return sum == 0.0 ? null : sum;
            }
            set {}
        }

        [DataMember()]
        public double? MonthEplus { get; set; }

        [DataMember()]
        [Newtonsoft.Json.JsonConverter(typeof(NullListToEmptyStringConverter))]
        public IList<double?> DailyEplus { get; set; }
        [JsonIgnore]
        public virtual double? Eminus
        {
            get
            {
                var result = new Nullable<double>(0);
                if (Items != null)
                    foreach (IBalansItem item in Items)
                    {
                        var value = item.Eminus;
                        if (value.HasValue)
                            result += value;
                    }
                return result == 0 ? null : result;
            }

            set { throw new InvalidOperationException("Свойство Eminus только для чтения. Тип: " + GetType() + ", элемент: " + Name); }
        }
        [JsonIgnore]
        public double? AddToEminus
        {
            get
            {
                if (Items == null) return null;
                double? sum = Items.Sum((i) => (i.AddToEminus != null && i.AddToEminus.HasValue) ? i.AddToEminus.Value : 0.0);
                return sum == 0.0 ? null : sum;
            }
            set {  }
        }

        [DataMember()]
        public double? MonthEminus { get; set; }

        [DataMember()]
        [Newtonsoft.Json.JsonConverter(typeof(NullListToEmptyStringConverter))]
        public IList<double?> DailyEminus { get; set; }
        [JsonIgnore]
        public virtual double? EnergyIn
        {
            get
            {
                Double? value = 0.0;
                var v = VvodaIn;
                value += v == null ? 0.0 : v;
                var f = FideraIn;
                value += f == null ? 0.0 : f;
                //Double add = (AddToEplus != null && AddToEplus.HasValue) ? AddToEplus.Value : 0.0;
                return (value.HasValue == false || value == 0.0) ? null : value;// + add;
            }
        }
        [JsonIgnore]
        public virtual double? EnergyOut
        {
            get
            {
                Double? value = 0.0;
                var v = VvodaOut;
                value += v == null ? 0.0 : v;
                var f = FideraOut;
                value += f == null ? 0.0 : f;
                if (value.HasValue == false || value == 0.0)
                    return null;
                var tsn = Tsn;
                //Double add = (AddToEminus != null && AddToEminus.HasValue) ? AddToEminus.Value : 0.0;
                return (tsn != null && tsn.HasValue) ? value + tsn /*+ add*/ : value;// + add;
            }
        }

        public virtual double? VvodaIn { get; set; }
        public virtual double? VvodaOut { get; set; }

        public virtual double? Tsn { get; set; }

        public virtual double? FideraIn { get; set; }
        public virtual double? FideraOut { get; set; }

        public virtual double? Unbalance { get { return null; } }
        public virtual double? PercentageOfUnbalance { get { return null; } }
        [DataMember()]
        public virtual double? MaximumAllowableUnbalance
        {
            get { return _maximumAllowableUnbalance; }
            set
            {
                _maximumAllowableUnbalance = null;
                RaisePropertyChanged("MaximumAllowableUnbalance");
            }
        }

        [DataMember()]
        public ElementTypes Type { get; set; }

        [DataMember()]
        public ICollection<IBalansItem> Children { get; set; }

        public IList<IBalansItem> Items { get; private set; }

        public void UpdateChildren()
        {
            if (Children != null)
            {
                var items = new List<IBalansItem>();
                foreach (IBalansItem item in Children)
                {
                    if (item is IBalansGroup)
                        (item as IBalansGroup).UpdateChildren();
                    item.PropertyChanged += Item_PropertyChanged;
                }
                ProcessBalansItemsToFloatCollection(this, ref items);
                Items = items;
            }
        }

        private void ProcessBalansItemsToFloatCollection(IBalansGroup item, ref List<IBalansItem> collection)
        {
            if (item == null) return;
            if (item.Children != null)
                foreach (IBalansItem child in item.Children)
                {
                    var group = child as IBalansGroup;
                    if (group == null && (item is SubstationAuxiliary || (item is SubstationSection && (item as SubstationSection).IsLowVoltage)))
                    {
                        if (child.Type == Model.ElementTypes.FIDER ||
                            child.Type == Model.ElementTypes.POWERTRANSFORMER ||
                            child.Type == Model.ElementTypes.UNITTRANSFORMER ||
                            child.Type == Model.ElementTypes.UNITTRANSFORMERBUS)
                        {
                            collection.Add(child);
                        }
                    }
                    else
                        ProcessBalansItemsToFloatCollection(group, ref collection);
                }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // TODO: переделать оповещение
            if (e.PropertyName == "Status")
            {
                RaisePropertyChanged("Status");
                RaisePropertyChanged("Progress");
            }
            if (e.PropertyName == "AddToEplus")
            {
                RaisePropertyChanged("AddToEplus");
            }
            if (e.PropertyName == "AddToEminus")
            {
                RaisePropertyChanged("AddToEminus");
            }
            if (e.PropertyName == "Correction")
            {
                RaisePropertyChanged("Correction");
            }
            if (e.PropertyName == "Eplus")
            {
                RaisePropertyChanged("Eplus");
                RaisePropertyChanged("FideraIn");
                RaisePropertyChanged("VvodaIn");
                RaisePropertyChanged("EnergyIn");
                RaisePropertyChanged("Unbalance");
                RaisePropertyChanged("PercentageOfUnbalance");
            }
            if (e.PropertyName == "Eminus")
            {
                RaisePropertyChanged("Eminus");
                RaisePropertyChanged("FideraOut");
                RaisePropertyChanged("VvodaOut");
                RaisePropertyChanged("EnergyOut");
                RaisePropertyChanged("Unbalance");
                RaisePropertyChanged("PercentageOfUnbalance");
            }
        }

        private DataStatus _status = DataStatus.Wait;

        [DataMember()]
        public DataStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;

                if (_status == DataStatus.Wait)
                {
                    if (Items != null)
                    foreach (var item in Items)
                        item.Status = DataStatus.Wait;
                }

                RaisePropertyChanged("Status");
                RaisePropertyChanged("Progress");
            }
        }

        public int Progress
        {
            get
            {
                if (Items == null) return 0;
                if (Items.Count == 0) return 0;
                int processedCount = Items.Where(i => i.Status == DataStatus.Processed).Count();
                return 100 * processedCount / Items.Count;
            }
        }

        public bool NotFullData
        {
            get
            {
                return (Children != null && Children.Count > 0) ? Children.Any((item) => item.NotFullData) : false;
            }
        }

        public bool NotFullDataPlus
        {
            get
            {
                return (Children != null && Children.Count > 0) ? Children.Any((item) => item.NotFullDataPlus) : false;
            }
        }

        public bool NotFullDataMinus
        {
            get
            {
                return (Children != null && Children.Count > 0) ? Children.Any((item) => item.NotFullDataMinus) : false;
            }
        }

        public string DataPlusStatus { get { return null; } }
        public string DataMinusStatus { get { return null; } }

        [DataMember()]
        public string Description {
            get { return _description; }
            set { _description = value; RaisePropertyChanged("Description"); } }

        public Substation Substation { get; private set; }
        public void SetSubstation(Substation substation)
        {
            Substation = substation;
        }

        public void Clear()
        {
            if (Items != null)
                foreach (IBalansItem item in Items)
                    item.Clear();
            RaisePropertiesChanged();
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

        public IList<Value> DailyEplusValues
        {
            get
            {
                if (DailyEplus == null) return null;
                var max = DailyEplus.Max();
                if (max == null) return null;

                IList<Value> result = new List<Value>();
                foreach (double? item in DailyEplus)
                    result.Add(item == null
                        ? new Value
                        { DoubleValue = 0, PercentValue = 0, Status = ValueStatus.Missing }
                        : new Value
                        { DoubleValue = item.Value, PercentValue = item.Value / max, Status = ValueStatus.Normal });
                return result;
            }
        }

        public IList<Value> DailyEminusValues
        {
            get
            {
                if (DailyEminus == null) return null;
                var max = DailyEminus.Max();
                if (max == null) return null;
                IList<Value> result = new List<Value>();
                foreach (double? item in DailyEminus)
                    result.Add(item == null
                        ? new Value
                        { DoubleValue = 0, PercentValue = 0, Status = ValueStatus.Missing }
                        : new Value
                        { DoubleValue = item.Value, PercentValue = item.Value / max, Status = ValueStatus.Normal });
                return result;
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
                    value = values.Average((i) => i.DoubleValue.Value);
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

        #endregion VALUES
        [OnDeserializing()]
        private void OnDeserializingMethod(StreamingContext context)
        {
            ;
        }

        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context)
        {
            UpdateChildren();
        }

        public virtual IBalansItem Copy()
        {
            throw new NotImplementedException();
        }
        public bool UseMonthValue {
            get
            {
                return Items == null
                  ? false
                  : Items.All(i => i.UseMonthValue);
            }
            set
            {
                if (Items == null) return;
                foreach (var item in Items) item.UseMonthValue = value;
                RaisePropertiesChanged();
            }
        }
        public double? DayEminusValue { get; set; }
        public double? DayEplusValue { get; set; }

        private void RaisePropertiesChanged()
        {
            RaisePropertyChanged("EnergyIn");
            RaisePropertyChanged("EnergyIn");
            RaisePropertyChanged("EnergyOut");
            RaisePropertyChanged("Unbalance");
            RaisePropertyChanged("PercentageOfUnbalance");
            RaisePropertyChanged("NotFullDataPlus");
            RaisePropertyChanged("NotFullDataMinus");
            RaisePropertyChanged("AddToEplus");
            RaisePropertyChanged("AddToEminus");
        }
    }
}