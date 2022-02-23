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
                this.Status = "подготовка";

                // CreateView();
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
                    /* this.RaisePropertyChanged(nameof(this.ReportTitle));
                    this.OnDateChanged(); */
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
            this.DatePeriods = datePeriods;
            this.SelectedPeriod = this.DatePeriods.LastOrDefault().Value;

            var data = MainViewModel?.Data?.Payments.SelectMany(i => i.Value);

            if (data == null)
                return;

            var years = data
                .GroupBy(i => i.ПериодОплаты.Year, j => j, (key, el) => new { Year = key, Items = el })
                .ToList();
            foreach (var year in years)
            {
                var months = year.Items
                    .GroupBy(i => i.ПериодОплаты.Month, j => j, (key, el) => new { Month = key, Items = el })
                    .ToList();
                foreach (var month in months)
                {
                    datePeriods.Add(new KeyValuePair<string, DateOnly>(
                        new DateOnly(year.Year, month.Month, 1).ToString("MMMM yyyy", AppSettings.CurrentCulture),
                        new DateOnly(year.Year, month.Month, 1).AddDays(-1).AddMonths(1)));
                }
            }
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
            var allMeters = MainViewModel?.Data?.Meters.Where(i => i.Удалён == false);
            if (allMeters == null)
            {
                return;
            }

            var allPayments = MainViewModel?.Data?.Payments.SelectMany(i => i.Value);
            if (allPayments == null)
            {
                return;
            }

            const string empty = "(пусто)";

            IList<uint> getAbonentsConsuptions(IList<Meter> metersList)
            {
                IList<uint> result = new List<uint>(metersList.Count);
                IList<Payment> payments;
                foreach (var meter in metersList)
                {
                    payments = allPayments.Where(p => p.Лицевой == meter.Лицевой).ToList();
                }
            }

            IList<FiderAnalizTreeItem> groupMetersByProperty(IEnumerable<Meter> metersList, string propName, FiderAnalizTreeItemType itemType)
            {
                // группируем список по значению свойства
                List<IGrouping<object, Meter>> groups = metersList
                    .GroupBy(i => ModelHelper.MeterGetPropertyValue(i, propName))
                    .ToList();
                if (groups.Count >= 1)
                {
                    List<FiderAnalizTreeItem> items = new(groups.Count);
                    foreach (IGrouping<object, Meter> group in groups)
                    {
                        FiderAnalizTreeItem item = new();
                        string header = group.Key == null || string.IsNullOrWhiteSpace(group.Key.ToString()) ? empty : group.Key.ToTrimmedString();
                        item.Header = header;
                        item.ChildMeters = group.ToList();
                        item.Type = itemType;
                        items.Add(item);
                    }

                    return items.OrderBy(i => i.Header).ToList();
                }
                else
                {
                    return null;
                }
            }

            IList<Meter> list = null;
            IList<FiderAnalizTreeItem> substationsNodes = groupMetersByProperty(allMeters, "Подстанция", FiderAnalizTreeItemType.Substation);
            if (substationsNodes != null)
            {
                foreach (FiderAnalizTreeItem substation in substationsNodes)
                {
                    IList<FiderAnalizTreeItem> fiders = groupMetersByProperty(substation.ChildMeters, "Фидер10", FiderAnalizTreeItemType.Fider10);
                    if (fiders != null)
                    {
                        substation.AddChildren(fiders);
                        foreach (FiderAnalizTreeItem fider in substation.Children)
                        {
                            IList<FiderAnalizTreeItem> tps = groupMetersByProperty(fider.ChildMeters, "ТП", FiderAnalizTreeItemType.TP);
                            if (tps != null)
                            {
                                fider.AddChildren(tps);
                                foreach (FiderAnalizTreeItem tp in fider.Children)
                                {
                                    IList<FiderAnalizTreeItem> fiders04 = groupMetersByProperty(tp.ChildMeters, "Фидер04", FiderAnalizTreeItemType.Fider04);
                                    if (fiders04 != null)
                                    {
                                        tp.AddChildren(fiders04);
                                    }
                                    else
                                    {
                                        tp.NotBindingAbonentsCount = tp.ChildMetersCount;
                                        tp.NotBindingAbonentsConsumption = getAbonentsConsuptions()
                                    }
                                }

                                list = fider.ChildMeters.Where(i => i.ТП == null || (i.ТП != null && i.ТП.IsEmpty)).ToList();
                                fider.NotBindingAbonentsCount = (uint)fider.Children.Cast<FiderAnalizTreeItem>().Sum(i => i.NotBindingAbonentsCount);

                                if (list.Count > 0)
                                {
                                    IEnumerable<FiderAnalizTreeItem> emptyNodes = fider.Children.Cast<FiderAnalizTreeItem>().Where(i => string.Equals(i.Header, empty, AppSettings.StringComparisonMethod));
                                    if (!emptyNodes.Any())
                                    {
                                        fider.Children.Insert(0, item: new FiderAnalizTreeItem(
                                            fider,
                                            empty,
                                            list,
                                            FiderAnalizTreeItemType.TP));
                                    }
                                }
                            }
                            else
                            {
                                fider.NotBindingAbonentsCount = fider.ChildMetersCount;
                            }
                        }

                        list = substation.ChildMeters.Where(i => string.IsNullOrWhiteSpace(i.Фидер10)).ToList();
                        substation.NotBindingAbonentsCount = (uint)substation.Children.Cast<FiderAnalizTreeItem>().Sum(i => i.NotBindingAbonentsCount);

                        if (list.Count > 0)
                        {
                            IEnumerable<FiderAnalizTreeItem> emptyNodes = substation.Children.Cast<FiderAnalizTreeItem>().Where(i => string.Equals(i.Header, empty, AppSettings.StringComparisonMethod));
                            if (!emptyNodes.Any())
                            {
                                substation.Children.Insert(0, new FiderAnalizTreeItem(substation,
                                                                                     empty,
                                                                                     list,
                                                                                     FiderAnalizTreeItemType.Fider10)
                                {
                                    Parent = substation,
                                });
                            }
                        }
                    }
                    else
                    {
                        substation.NotBindingAbonentsCount = substation.ChildMetersCount;
                    }
                }
            }

            list = allMeters
                .Where(i =>
                    string.IsNullOrWhiteSpace(i.Подстанция) |
                    string.IsNullOrWhiteSpace(i.Фидер10) |
                    (i.ТП == null || (i.ТП != null && i.ТП.IsEmpty)) |
                    i.Фидер04.HasValue == false)
                .ToList();

            FiderAnalizTreeItem root = new()
            {
                IsExpanded = true,
                Type = FiderAnalizTreeItemType.Group,
                Header = "РЭС",
                ChildMeters = new List<Meter>(allMeters),
                ChildMetersCount = (uint)allMeters.Count(),
                NotBindingAbonentsCount = (uint)list.Count,
            };
            if (list.Count > 0)
            {
                root.Children.Add(new FiderAnalizTreeItem(
                    root,
                    "Не полная привязка",
                    list,
                    FiderAnalizTreeItemType.Group));
            }

            if (!substationsNodes.Cast<FiderAnalizTreeItem>().Any(i => string.Equals(i.Header, empty, AppSettings.StringComparisonMethod)))
            {
                list = allMeters.Where(i => string.IsNullOrWhiteSpace(i.Подстанция)).ToList();
                if (list.Count > 0)
                {
                    root.Children.Add(new FiderAnalizTreeItem(
                        root,
                        empty,
                        list,
                        FiderAnalizTreeItemType.Substation));
                }
            }

            root.AddChildren(substationsNodes);

            this.ModelTree = new FiderAnalizTreeModel(new ObservableCollection<FiderAnalizTreeItem>(root.Children));
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
