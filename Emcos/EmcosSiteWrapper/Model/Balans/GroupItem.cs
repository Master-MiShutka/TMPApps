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
    public abstract class GroupItem : PropertyChangedBase, IBalansGroup, IProgress
    {
        #region Fields

        private DataStatus _status = DataStatus.Wait;

        private int _id;
        private string _code;
        private string _name;

        private string _description;
        private double? _maximumAllowableUnbalance = null;

        #endregion

        public GroupItem()
        {
            Children = new ObservableCollection<IBalansItem>();
        }

        #region Private methods

        private void ProcessBalansItemsToFloatCollection(IBalansGroup item, ref List<IBalansItem> collection)
        {
            if (item == null) return;
            if (item.Children != null)
                foreach (IBalansItem child in item.Children)
                {
                    var group = child as IBalansGroup;
                    if (group == null && (item is SubstationAuxiliary || (item is SubstationSection && (item as SubstationSection).IsLowVoltage)))
                    {
                        if (child.ElementType == Model.ElementTypes.FIDER ||
                            child.ElementType == Model.ElementTypes.POWERTRANSFORMER ||
                            child.ElementType == Model.ElementTypes.UNITTRANSFORMER ||
                            child.ElementType == Model.ElementTypes.UNITTRANSFORMERBUS)
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

        #endregion

        #region Public methods

        public void SetSubstation(Substation substation)
        {
            Substation = substation;
        }
        /// <summary>
        /// Каскадная очистка группы
        /// </summary>
        public void Clear()
        {
            if (Items != null)
                foreach (IBalansItem item in Items)
                    item.Clear();
            RaisePropertiesChanged();
        }
        /// <summary>
        /// Возвращает копию элемента
        /// </summary>
        /// <returns></returns>
        public virtual IBalansItem Copy()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Подписка на изменение свойств дочерних элементов и создание плоского списка дочерних элементов
        /// </summary>
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


        #endregion

        #region Properties

        #region IEmcosPoint implementation

        [DataMember()]
        public int Id { get { return _id; } set { SetProp(ref _id, value, "Id"); } }

        [DataMember()]
        public string Name { get { return _name; } set { SetProp(ref _name, value, "Name"); } }

        [DataMember()]
        public string Code { get { return _code; } set { SetProp(ref _code, value, "Code"); } }

        [IgnoreDataMember]
        public string TypeCode { get; set; }

        public virtual ElementTypes ElementType { get; }

        [IgnoreDataMember]
        public string EcpName { get; set; }

        [DataMember()]
        public string Description
        {
            get { return _description; }
            set { SetProp(ref _description, value, "Description"); }
        }

        #endregion

        #region IBalansItem implementation
        /// <summary>
        /// Статус данных
        /// </summary>
        [DataMember()]
        public DataStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                SetProp(ref _status, value, "Status");

                if (_status == DataStatus.Wait)
                {
                    if (Items != null)
                        foreach (var item in Items)
                            item.Status = DataStatus.Wait;
                }

                RaisePropertyChanged("Progress");
            }
        }
        /// <summary>
        /// Ссылка на подстанцию
        /// </summary>
        public Substation Substation { get; private set; }
        /// <summary>
        /// Используются ли данные за месяц
        /// </summary>
        public bool UseMonthValue
        {
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
        /// <summary>
        /// Возвращает описание произведенных корректировок
        /// </summary>
        public string Correction
        {
            get
            {

            }
        }

        public ActiveEnergy ActiveEnergy { get; set; }
        public ReactiveEnergy ReactiveEnergy { get; set; }

        #endregion

        public BalanceFormula Formula { get; set; }

        [DataMember()]
        public ICollection<IBalansItem> Children { get; set; }

        #endregion



        public IList<IBalansItem> Items { get; private set; }





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

    }
}