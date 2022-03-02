namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;
    using TMP.Extensions;
    using TMP.Shared;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    using TMP.WORK.AramisChetchiki.Model;

    public class PaymentsAndPofiderAnalizViewModel : BaseViewModel
    {
        #region Fields

        private MTObservableCollection<IMatrix> pivotCollection = new();
        private FiderAnalizTreeModel modelTree;
        private FiderAnalizTreeItem selectedItemNode;
        private DateOnly? selectedPeriod;
        private ICollection<KeyValuePair<string, DateOnly>> datePeriods;
        private string description = string.Empty;

        #endregion Fields

        #region Constructor
        public PaymentsAndPofiderAnalizViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.IsBusy = false;
                return;
            }

            // this.Data = MainViewModel?.Data?.Payments.SelectMany(i => i.Value);

            this.CommandPrint = null;

            this.IsBusy = true;
            this.Init();
        }

        #endregion

        #region Properties

        public FiderAnalizTreeModel ModelTree { get => this.modelTree; private set => this.SetProperty(ref this.modelTree, value); }

        public FiderAnalizTreeItem SelectedItemNode { get => this.selectedItemNode; set => this.SetProperty(ref this.selectedItemNode, value); }

        public DateOnly? SelectedPeriod
        {
            get => this.selectedPeriod;
            set
            {
                if (this.SetProperty(ref this.selectedPeriod, value))
                {
                    this.IsBusy = true;
                    Task.Run(() =>
                    {
                        this.CreatePivots();
                        this.BuildTreeModel();
                    })
                    .ContinueWith(t => this.IsBusy = false);
                }
            }
        }

        public ICollection<KeyValuePair<string, DateOnly>> DatePeriods
        {
            get => this.datePeriods;
            private set => this.SetProperty(ref this.datePeriods, value);
        }

        public MTObservableCollection<IMatrix> PivotCollection
        {
            get => this.pivotCollection;
            private set => this.SetProperty(ref this.pivotCollection, value);
        }

        public string Description
        {
            get => this.description;
            private set => this.SetProperty(ref this.description, value);
        }

        #region Commands

        #endregion

        #endregion

        #region Private methods

        private void Init()
        {
            this.IsBusy = true;
            Task.Run(() =>
            {
                this.BuildPeriods();
                this.CreatePivots();
                this.BuildTreeModel();
            })
            .ContinueWith(t => this.IsBusy = false, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void BuildPeriods()
        {
            List<KeyValuePair<string, DateOnly>> datePeriods = new List<KeyValuePair<string, DateOnly>>();

            var data = MainViewModel?.Data?.Payments.SelectMany(i => i.Value);

            if (data == null)
                return;

            var years = data
                .Where(i => i.ПериодОплаты != DateOnly.MinValue)
                .GroupBy(i => i.ПериодОплаты.Year, j => j, (key, el) => new { Year = key, Items = el })
                .OrderBy(i => i.Year)
                .ToList();
            foreach (var year in years)
            {
                var months = year.Items
                    .GroupBy(i => i.ПериодОплаты.Month, j => j, (key, el) => new { Month = key, Items = el })
                    .OrderBy(i => i.Month)
                    .ToList();
                foreach (var month in months)
                {
                    datePeriods.Add(new KeyValuePair<string, DateOnly>(
                        new DateOnly(year.Year, month.Month, 1).ToString("MMMM yyyy", AppSettings.CurrentCulture),
                        new DateOnly(year.Year, month.Month, 1).AddDays(-1).AddMonths(1)));
                }
            }

            this.DatePeriods = datePeriods;
            this.SelectedPeriod = this.DatePeriods.LastOrDefault().Value;
        }

        private void CreatePivots()
        {
            this.PivotCollection.Clear();

            foreach (IMatrix pivot in SummaryInfoHelper.GetEnergyPowerSuppyPivots(MainViewModel.Data, this.SelectedPeriod.Value))
            {
                this.PivotCollection.Add(pivot);
            }
        }

        private void BuildTreeModel()
        {
            if (this.selectedPeriod.HasValue == false || this.selectedPeriod.Value == DateOnly.MinValue)
            {
                return;
            }

            this.Status = "построение модели";

            var allMeters = MainViewModel?.Data?.Meters.Where(i => i.Удалён == false);
            if (allMeters == null)
            {
                return;
            }

            this.DetailedStatus = "группировка оплат";
            var allPayments = MainViewModel?.Data?.Payments
                .SelectMany(i => i.Value)
                .Where(i => i.ПериодОплаты.Year == this.selectedPeriod.Value.Year)
                .Where(i => i.ПериодОплаты.Month == this.selectedPeriod.Value.Month)
                .GroupBy(i => i.Лицевой, p => p)
                .ToDictionary(i => i.Key, p => p.ToList());
            if (allPayments == null)
            {
                return;
            }

            uint? GetMeterConsumption(Meter meter)
            {
                if (allPayments.ContainsKey(meter.Лицевой))
                {
                    uint summ = (uint)allPayments[meter.Лицевой].Sum(i => i.РазностьПоказаний);
                    return summ;
                }

                return null;
            }

            IList<FiderAnalizTreeItem> GroupMetersByProperty(IEnumerable<FiderAnalizMeter> metersList, string propName, FiderAnalizTreeItemType itemType)
            {
                // группируем список по значению свойства
                List<IGrouping<object, FiderAnalizMeter>> groups = metersList
                    .GroupBy(i => ModelHelper.FiderAnalizMeterGetPropertyValue(i, propName))
                    .ToList();
                if (groups.Count >= 1)
                {
                    List<FiderAnalizTreeItem> items = new(groups.Count);
                    foreach (IGrouping<object, FiderAnalizMeter> group in groups)
                    {
                        string header = group.Key == null || string.IsNullOrWhiteSpace(group.Key.ToString()) ? FiderAnalizTreeItem.EmptyHeader : group.Key.ToTrimmedString();
                        FiderAnalizTreeItem item = new(null, header, itemType, group.ToList());
                        items.Add(item);
                    }

                    return items.OrderBy(i => i.Header).ToList();
                }
                else
                {
                    return null;
                }
            }

            this.DetailedStatus = "вычисление потребление э/э счётчиками";
            IList<FiderAnalizMeter> allFiderAnalizMeters = allMeters.Select(i => new FiderAnalizMeter(i, GetMeterConsumption(i))).ToList();

            IList<FiderAnalizTreeItem> substationsNodes = GroupMetersByProperty(allFiderAnalizMeters, nameof(FiderAnalizMeter.Substation), FiderAnalizTreeItemType.Substation);
            if (substationsNodes != null)
            {
                foreach (FiderAnalizTreeItem substation in substationsNodes)
                {
                    this.DetailedStatus = $"ПС '{substation.Header}'";

                    IList<FiderAnalizTreeItem> fiders10 = GroupMetersByProperty(substation.ChildMeters, nameof(FiderAnalizMeter.Fider10), FiderAnalizTreeItemType.Fider10);
                    if (fiders10 != null)
                    {
                        substation.AddChildren(fiders10);
                        foreach (FiderAnalizTreeItem fider10 in substation.Children)
                        {
                            IList<FiderAnalizTreeItem> tps = GroupMetersByProperty(fider10.ChildMeters, nameof(FiderAnalizMeter.Tp), FiderAnalizTreeItemType.TP);
                            if (tps != null)
                            {
                                fider10.AddChildren(tps);
                                foreach (FiderAnalizTreeItem tp in fider10.Children)
                                {
                                    IList<FiderAnalizTreeItem> fiders04 = GroupMetersByProperty(tp.ChildMeters, nameof(FiderAnalizMeter.Fider04), FiderAnalizTreeItemType.Fider04);
                                    if (fiders04 != null)
                                    {
                                        tp.AddChildren(fiders04);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.DetailedStatus = "завершение ...";

            IList<FiderAnalizMeter> list = allFiderAnalizMeters
                .Where(i =>
                    string.IsNullOrWhiteSpace(i.Substation) |
                    string.IsNullOrWhiteSpace(i.Fider10) |
                    string.IsNullOrWhiteSpace(i.Tp) |
                    string.IsNullOrWhiteSpace(i.Fider04))
                .ToList();

            FiderAnalizTreeItem root = new(null, "РЭС", FiderAnalizTreeItemType.Group, allFiderAnalizMeters)
            {
                IsExpanded = true,
                NotBindingAbonentsCount = (uint)list.Count,
                NotBindingAbonentsConsumption = (uint)list.Sum(i => i.Consumption),
            };
            if (list.Count > 0)
            {
                root.Children.Add(new FiderAnalizTreeItem(
                    root,
                    "Не полная привязка",
                    FiderAnalizTreeItemType.Group,
                    list));
            }

            if (!substationsNodes.Cast<FiderAnalizTreeItem>().Any(i => string.Equals(i.Header, FiderAnalizTreeItem.EmptyHeader, AppSettings.StringComparisonMethod)))
            {
                list = allFiderAnalizMeters.Where(i => string.IsNullOrWhiteSpace(i.Substation)).ToList();
                if (list.Count > 0)
                {
                    root.Children.Add(new FiderAnalizTreeItem(
                        root,
                        FiderAnalizTreeItem.EmptyHeader,
                        FiderAnalizTreeItemType.Substation,
                        list));
                }
            }

            root.AddChildren(substationsNodes);

            this.ModelTree = new FiderAnalizTreeModel(new ObservableCollection<FiderAnalizTreeItem>(root.Children));

            this.Status = null;
            this.DetailedStatus = null;
        }

        /* protected override ICollectionView BuildAndGetView()
        {
            this.IsBusy = true;
            ICollectionView view = CollectionViewSource.GetDefaultView(this.Data);
            using (view.DeferRefresh())
            {
                view.SortDescriptions.Add(new SortDescription("Период", ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("Лицевой", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("Адрес", ListSortDirection.Ascending));
                view.Filter = this.Filter;
            }

            (view as INotifyPropertyChanged).PropertyChanged += this.View_PropertyChanged;

            this.IsBusy = false;

            return view;
        }

        private void View_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Count")
            {
                this.CollectionViewItemsCount = (sender as ListCollectionView).Count;
            }
        }

        protected override bool Filter(object obj)
        {
            bool result = true;

            if (obj is not ElectricitySupply item)
            {
                return false;
            }

            if (this.SelectedPeriod.HasValue)
            {
                result = item.Период == this.SelectedPeriod;
            }

            return result;
        }

        private void OnDateChanged()
        {
            if (this.View != null)
            {
                this.View.Refresh();
                this.CreatePivots();

                this.RaisePropertyChanged(nameof(this.ElectricitySupplySumm));
                this.RaisePropertyChanged(nameof(this.ElectricitySupplySummWithЗадолженность));
            }
        }

        public override string ReportTitle => string.Format(AppSettings.CurrentCulture, "Сведения по полезному отпуску населению за период с {0} по {1}",
            this.SelectedPeriod.Value.AddDays(1).AddMonths(-1).ToShortDateString(), this.SelectedPeriod.Value.ToShortDateString());
        */

        #endregion

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0454");
            return guid.GetHashCode();
        }
    }
}
