namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using TMP.Extensions;
    using TMP.Shared.Commands;
    using TMP.Shared.Tree;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    using TMP.WORK.AramisChetchiki.Model;

    public class PaymentsAndPofiderAnalizViewModel : BaseViewModel
    {
        #region Fields

        private ObservableCollection<IMatrix> pivotCollection = new();
        private string treeSearchString;
        private IEnumerable<FiderAnalizTreeItem> treeNodes;
        private FiderAnalizTreeItem selectedNode;
        private DateOnly? selectedPeriod;
        private ICollection<KeyValuePair<string, DateOnly>> datePeriods;
        private string description = string.Empty;

        private AbonentsBindingTreeMode abonentsBindingTreeMode = AbonentsBindingTreeMode.Full;

        private IEnumerable<Meter> allMeters;
        private IList<FiderAnalizMeter> allFiderAnalizMeters;

        #endregion Fields

        #region Constructor
        public PaymentsAndPofiderAnalizViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.IsBusy = false;
                return;
            }

            WindowWithDialogs.BaseApplication.InvokeInUIThread(() => System.Windows.Data.BindingOperations.EnableCollectionSynchronization(this.pivotCollection, new object()));

            // this.Data = MainViewModel?.Data?.Payments.SelectMany(i => i.Value);

            this.CommandViewDetailsBySelectedItem = new DelegateCommand<object>(this.DoViewDetailsBySelectedItem);
            this.CommandChangeViewKind = new DelegateCommand<AbonentsBindingTreeMode>(this.DoChangeViewKind);

            this.CommandPrint = null;

            this.IsBusy = true;
            this.Init();
        }

        #endregion

        #region Properties

        public string TreeSearchString
        {
            get => this.treeSearchString;

            set
            {
                if (this.SetProperty(ref this.treeSearchString, value))
                {
                    this.ApplyFilter();
                }
            }
        }

        public IEnumerable<FiderAnalizTreeItem> TreeNodes { get => this.treeNodes; private set => this.SetProperty(ref this.treeNodes, value); }

        /// <summary>
        /// Выбранный в дереве узел
        /// </summary>
        public FiderAnalizTreeItem SelectedNode
        {
            get
            {
                return this.selectedNode;
            }

            set
            {
                if (this.SetProperty(ref this.selectedNode, value) && value != null)
                {
                    this.RaisePropertyChanged(nameof(this.ChildMeters));
                }
            }
        }

        public IList<FiderAnalizMeter> ChildMeters => this.SelectedNode?.ChildMeters;

        public DateOnly? SelectedPeriod
        {
            get => this.selectedPeriod;
            set
            {
                if (this.SetProperty(ref this.selectedPeriod, value))
                {
                    this.RebuildTree();
                }
            }
        }

        public ICollection<KeyValuePair<string, DateOnly>> DatePeriods
        {
            get => this.datePeriods;
            private set => this.SetProperty(ref this.datePeriods, value);
        }

        public ObservableCollection<IMatrix> PivotCollection
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

        public ICommand CommandViewDetailsBySelectedItem { get; }

        public ICommand CommandChangeViewKind { get; }

        #endregion

        #endregion

        #region Private methods

        private void ApplyFilter()
        {
            this.IsBusy = true;
            Task task = Task.Run(() =>
            {
                foreach (ITreeNode child in this.treeNodes)
                {
                    child.ApplyCriteria(this.treeSearchString, new Stack<ITreeNode>());
                }
            });
            task.ContinueWith(t =>
            {
                this.IsBusy = false;
            });
        }

        private void DoViewDetailsBySelectedItem(object selectedItem)
        {
            if (selectedItem == null || selectedItem is FiderAnalizMeter fiderAnalizMeter == false)
            {
                return;
            }

            var meter = MainViewModel?.Data?.Meters.Where(i => i.Удалён == false && i.Лицевой == fiderAnalizMeter.Id).FirstOrDefault();

            if (meter == null)
            {
                return;
            }

            Controls.MeterView control = new Controls.MeterView
            {
                DataContext = new ViewModel.MeterViewViewModel(meter),
            };
            Action dialogCloseAction = this.ShowCustomDialog(control, "-= Подробная информация =-");
            control.CloseAction = dialogCloseAction;
        }

        private void DoChangeViewKind(AbonentsBindingTreeMode kind)
        {
            this.abonentsBindingTreeMode = kind;
            this.RebuildTree();
        }

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

        private void RebuildTree()
        {
            this.IsBusy = true;
            Task.Run(() =>
            {
                this.CreatePivots();
                this.BuildTreeModel();
            })
            .ContinueWith(t => this.IsBusy = false);
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

            this.datePeriods = datePeriods;
            this.selectedPeriod = this.DatePeriods.LastOrDefault().Value;
            this.RaisePropertyChanged(nameof(this.SelectedPeriod));
            this.RaisePropertyChanged(nameof(this.DatePeriods));
        }

        private void CreatePivots()
        {
            this.PivotCollection.Clear();

            foreach (IMatrix pivot in SummaryInfoHelper.GetPaymentsAndPofiderAnalizPivots(this.SelectedPeriod.Value, this.allFiderAnalizMeters))
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

            this.allMeters = MainViewModel?.Data?.Meters.Where(i => i.Удалён == false);
            if (this.allMeters == null)
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
                        FiderAnalizTreeItem item = new(null, header, itemType, group.OrderByDescending(m => m.Consumption).ToList());
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
            this.allFiderAnalizMeters = this.allMeters.Select(i => new FiderAnalizMeter(i, GetMeterConsumption(i))).ToList();

            this.DetailedStatus = "построение структуры ...";
            IList<FiderAnalizTreeItem> nodes = null;

            void BuildTree(FiderAnalizTreeItem item, FiderAnalizTreeItemType itemType)
            {
                IList<FiderAnalizMeter> meters = item == null ? this.allFiderAnalizMeters : item.ChildMeters;
                IList<FiderAnalizTreeItem> childs = null;
                FiderAnalizTreeItemType childType = FiderAnalizTreeItemType.None;
                string fieldName = nameof(FiderAnalizMeter.Substation);

                switch (itemType)
                {
                    case FiderAnalizTreeItemType.None:
                        childType = FiderAnalizTreeItemType.Substation;
                        fieldName = nameof(FiderAnalizMeter.Substation);
                        break;
                    case FiderAnalizTreeItemType.Substation:
                        childType = FiderAnalizTreeItemType.Fider10;
                        fieldName = nameof(FiderAnalizMeter.Fider10);
                        break;
                    case FiderAnalizTreeItemType.Fider10:
                        childType = FiderAnalizTreeItemType.TP;
                        fieldName = nameof(FiderAnalizMeter.Tp);
                        break;
                    case FiderAnalizTreeItemType.TP:
                        childType = FiderAnalizTreeItemType.Fider04;
                        fieldName = nameof(FiderAnalizMeter.Fider04);
                        break;
                    case FiderAnalizTreeItemType.Fider04:
                        childType = FiderAnalizTreeItemType.None;
                        break;
                }

                childs = GroupMetersByProperty(meters, fieldName, childType);
                if (item == null)
                {
                    nodes = childs;
                }

                if (childs != null && childType != FiderAnalizTreeItemType.None)
                {
                    item?.AddChildren(childs);
                    foreach (FiderAnalizTreeItem child in childs)
                    {
                        BuildTree(child, childType);
                    }
                }
            }

            switch (this.abonentsBindingTreeMode)
            {
                case AbonentsBindingTreeMode.Full:
                    BuildTree(null, FiderAnalizTreeItemType.None);
                    break;
                case AbonentsBindingTreeMode.WithoutSubstation:
                    BuildTree(null, FiderAnalizTreeItemType.Fider10);
                    break;
                case AbonentsBindingTreeMode.WithoutFider10:
                    BuildTree(null, FiderAnalizTreeItemType.TP);
                    break;
                default:
                    throw new NotImplementedException(nameof(this.abonentsBindingTreeMode));
            }

            this.DetailedStatus = "завершение ...";

            IList<FiderAnalizMeter> list = this.allFiderAnalizMeters
                .Where(i =>
                    string.IsNullOrWhiteSpace(i.Substation) |
                    string.IsNullOrWhiteSpace(i.Fider10) |
                    string.IsNullOrWhiteSpace(i.Tp) |
                    string.IsNullOrWhiteSpace(i.Fider04))
                .ToList();

            FiderAnalizTreeItem root = new(null, "РЭС", FiderAnalizTreeItemType.Group, this.allFiderAnalizMeters)
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

            if (!nodes.Cast<FiderAnalizTreeItem>().Any(i => string.Equals(i.Header, FiderAnalizTreeItem.EmptyHeader, AppSettings.StringComparisonMethod)))
            {
                list = this.allFiderAnalizMeters.Where(i => string.IsNullOrWhiteSpace(i.Substation)).ToList();
                if (list.Count > 0)
                {
                    root.Children.Add(new FiderAnalizTreeItem(
                        root,
                        FiderAnalizTreeItem.EmptyHeader,
                        FiderAnalizTreeItemType.Substation,
                        list));
                }
            }

            root.AddChildren(nodes);

            this.TreeNodes = new List<FiderAnalizTreeItem>(root.Children.Cast<FiderAnalizTreeItem>());

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
