using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

using TMP.Shared;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Базовый класс, представляющий группу элементов для составления баланса энергии
    /// </summary>
    [DataContract(Name = "GroupItem")]
    public class BalanceGroupItem : BalanceItem, IBalanceGroupItem, IProgress, ITreeModel
    {
        #region Fields


//        private double? _maximumAllowableUnbalance = null;

        #endregion

        public BalanceGroupItem()
        {
            Children = new BalanceGroupsCollection(this);

            ActiveEnergyBalance = new Balance<ActiveEnergy>(this);
            ReactiveEnergyBalance = new Balance<ReactiveEnergy>(this);
        }

        public BalanceGroupItem(IEnumerable<IHierarchicalEmcosPoint> children) : this()
        {
            if (children != null)
            {
                foreach (var child in children)
                    Children.Add(child);
            }
        }

        public BalanceGroupItem(HierarchicalEmcosPointCollection children)
        {
            if (children != null)
            {
                foreach (var child in children)
                    Children.Add(child);
            }
        }

        #region Private methods

        private void ProcessBalanceItemsToFloatCollection(IBalanceGroupItem item, ref List<IBalanceItem> collection)
        {
            if (item == null) return;
            if (item.Children != null)
                foreach (IBalanceItem child in item.Children)
                {
                    var group = child as IBalanceGroupItem;
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
                        ProcessBalanceItemsToFloatCollection(group, ref collection);
                }
        }

        protected override void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    if (item is IBalanceItem bi)
                        bi.PropertyChanged -= Child_PropertyChanged;
                }
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    if (item is IBalanceItem bi)
                        bi.PropertyChanged += Child_PropertyChanged;
                }

            // построение плоского списка
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (Items == null)
                    {
                        Items = new List<IBalanceItem>();
                    }
                    foreach (var item in e.NewItems)
                        Items.Add(item as IBalanceItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Items != null)
                        foreach (var item in e.NewItems)
                            Items.Remove(item as IBalanceItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    BuildFlatItemsCollection();
                    break;
                default:
                    break;
            }

            // при изменении коллекции пересчёт баланса
            ActiveEnergyBalance = new Balance<ActiveEnergy>(this);
            ReactiveEnergyBalance = new Balance<ReactiveEnergy>(this);
        }

        private void BuildFlatItemsCollection()
        {
            var items = new List<IBalanceItem>();
            ProcessBalanceItemsToFloatCollection(this, ref items);
            Items = items;
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Status":
                    RaisePropertyChanged("Status");
                    RaisePropertyChanged("Progress");
                    break;
                case "Substation":
                    break;
            default:
                    break;
            }
        }

        #endregion

        #region Public methods

        public void Cleanup()
        {
            ;
        }

        public void RecalculateBalance()
        {
            ActiveEnergyBalance = new Balance<ActiveEnergy>(this);
            ReactiveEnergyBalance = new Balance<ReactiveEnergy>(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Формула, описывающая баланс группы элементов
        /// </summary>
        [IgnoreDataMember]
        public BalanceFormula Formula
        {
            get;
            set;
        }

        [Magic]
        /// <summary>
        /// Плоский список дочерних элементов
        /// </summary>
        public IList<IBalanceItem> Items { get; private set; }

        /// <summary>
        /// Баланс активной энергии
        /// </summary>
        /// <remarks>
        /// Пересчёт происходит при  изменении коллекции дочерних элекментов или вызове метода 
        /// <see cref="RecalculateBalance"/>
        /// </remarks>
        [Magic]
        public Balance<ActiveEnergy> ActiveEnergyBalance { get; set; }
        /// <summary>
        /// Баланс реактивной энергии
        /// </summary>
        [Magic]
        public Balance<ReactiveEnergy> ReactiveEnergyBalance { get; set; }

        /// <summary>
        /// При обработке дочерних элементов - количество обработанных
        /// </summary>
        public override int Progress
        {
            get
            {
                if (Items == null) return 0;
                if (Items.Count == 0) return 0;
                int processedCount = Items.Where(i => i.Status == DataStatus.Processed).Count();
                return 100 * processedCount / Items.Count;
            }
        }

        #endregion
    }
}