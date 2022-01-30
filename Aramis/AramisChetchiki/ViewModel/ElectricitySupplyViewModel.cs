namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;
    using TMP.Shared;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    using TMP.WORK.AramisChetchiki.Model;

    public class ElectricitySupplyViewModel : BaseDataViewModel<ElectricitySupply>
    {
        #region Fields

        private MTObservableCollection<IMatrix> pivotCollection = new();

        private int collectionViewItemsCount;
        private DateOnly? selectedPeriod;
        private ICollection<KeyValuePair<string, DateOnly>> datePeriods;
        private string description = string.Empty;

        #endregion Fields

        #region Constructor
        public ElectricitySupplyViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.IsBusy = false;
                this.Status = "подготовка";

                // CreateView();
                return;
            }

            this.Data = MainViewModel?.Data?.ElectricitySupplyInfo;

            this.CommandPrint = null;

            this.IsBusy = true;
            this.Init();
        }

        public override IEnumerable<PlusPropertyDescriptor> PropertyDescriptors => AppSettings.GetElectricitySupplyPropertyDescriptors();

        #endregion

        #region Properties

        public int ElectricitySupplySumm
        {
            get
            {
                if (this.Data == null)
                {
                    return 0;
                }

                int summ = 0;
                foreach (ElectricitySupply item in this.View)
                {
                    summ += item.Полезный_отпуск;
                }

                return summ;
            }
        }

        public int ElectricitySupplySummWithЗадолженность
        {
            get
            {
                if (this.Data == null)
                {
                    return 0;
                }

                int summ = 0;
                foreach (ElectricitySupply item in this.View)
                {
                    summ += item.Полезный_отпуск + item.Задолженность;
                }

                return summ;
            }
        }

        public DateOnly? SelectedPeriod
        {
            get => this.selectedPeriod;
            set
            {
                if (this.SetProperty(ref this.selectedPeriod, value))
                {
                    this.RaisePropertyChanged(nameof(this.ReportTitle));
                    this.OnDateChanged();
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
                this.MakePeriods();
                this.CreatePivots();
            })
            .ContinueWith(t => this.IsBusy = false, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void MakePeriods()
        {
            List<KeyValuePair<string, DateOnly>> datePeriods = new List<KeyValuePair<string, DateOnly>>();
            this.DatePeriods = datePeriods;
            this.SelectedPeriod = this.DatePeriods.LastOrDefault().Value;

            if (this.Data == null)
                return;

            var years = this.Data
                .Where(i => i.Период.HasValue)
                .GroupBy(i => i.Период.Value.Year)
                .Select(i => new { i.Key, Values = i.ToList() })
                .ToList();
            foreach (var year in years)
            {
                var months = year.Values
                    .GroupBy(i => i.Период.Value.Month)
                    .Select(i => new { i.Key, Values = i.ToList() })
                    .ToList();
                foreach (var month in months)
                {
                    datePeriods.Add(new KeyValuePair<string, DateOnly>(
                        new DateOnly(year.Key, month.Key, 1).ToString("MMMM yyyy", AppSettings.CurrentCulture),
                        new DateOnly(year.Key, month.Key, 1).AddDays(-1).AddMonths(1)));
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

        protected override ICollectionView BuildAndGetView()
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

        #endregion

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0454");
            return guid.GetHashCode();
        }
    }
}
