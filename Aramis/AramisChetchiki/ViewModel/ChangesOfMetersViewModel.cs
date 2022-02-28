namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;

    public class ChangesOfMetersViewModel : BaseDataViewModel<ChangeOfMeter>
    {
        private DateTime? fromDate;
        private DateTime? toDate;
        private string personalAccountForFilter = string.Empty;
        private string abonentNameForFilter = string.Empty;
        private string oldCounterNumberForFilter = string.Empty;
        private string newCounterNumberForFilter = string.Empty;
        private string tPForFilter = string.Empty;

        private const string DEFAULT_DATA_GRID_MESSAGE = "В указанном временном периоде\nзамены счетчиков не производились";
        private string dataGridMessage = DEFAULT_DATA_GRID_MESSAGE;

        public ChangesOfMetersViewModel()
            : base(MainViewModel?.Data?.ChangesOfMeters.SelectMany(i => i.Value))
        {
            DateTime today = DateTime.Today;
            DateTime beginOfMonth = today.AddDays(1 - today.Day);
            this.fromDate = beginOfMonth.AddMonths(-2);
            this.toDate = beginOfMonth.AddMonths(-1).AddDays(-1);

            this.CommandSetSorting = new DelegateCommand(
                () =>
            {
                // TMPApplication.WpfDialogs.Contracts.ICustomContentDialog dialog;

                /*var control = new Controls.SelectorFieldsAndSortCollectionView(ref _tableColumns, this.View);
                dialog = _mainWindow.DialogCustom(control,
                    "-= Выбор полей, их порядок и сортировка =-",
                    TMPApplication.WpfDialogs.DialogMode.None);
                control.CloseAction = () => dialog.Close();
                dialog.Show();*/
            }, () => this.Data != null);

            this.CommandSetPeriod = new DelegateCommand<string>(
                tag =>
            {
                DateTime month = new DateTime(today.Year, today.Month, 1);
                switch (tag)
                {
                    case "this week":
                        int diff = today.DayOfWeek - DayOfWeek.Monday;
                        if (diff < 0)
                        {
                            diff += 7;
                        }

                        this.FromDate = today.AddDays(-1 * diff).Date;
                        this.ToDate = today;
                        break;
                    case "prev week":
                        diff = today.DayOfWeek - (DayOfWeek.Monday + 7);
                        if (diff < 0)
                        {
                            diff += 14;
                        }

                        this.FromDate = today.AddDays(-1 * diff).Date;
                        this.ToDate = today.AddDays(-1 * diff).AddDays(6);
                        break;
                    case "prev and this week":
                        diff = today.DayOfWeek - (DayOfWeek.Monday + 7);
                        if (diff < 0)
                        {
                            diff += 14;
                        }

                        this.FromDate = today.AddDays(-1 * diff).Date;
                        this.ToDate = today;
                        break;
                    case "this month":
                        this.FromDate = month;
                        this.ToDate = today;
                        break;
                    case "prev month":
                        this.FromDate = month.AddMonths(-1);
                        this.ToDate = month.AddDays(-1);
                        break;
                    case "prev and this month":
                        this.FromDate = month.AddMonths(-1);
                        this.ToDate = today;
                        break;
                }
                this.RaisePropertyChanged(nameof(this.DataFilter));
            }, (o) => this.Data != null);

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.IsBusy = false;
                this.Status = "подготовка";
                return;
            }

            if (this.Data != null && this.Data.Any())
            {
                DateOnly? lastDateTime = this.Data.Max(i => i.ДатаЗамены);

                if (lastDateTime.HasValue)
                {
                    DateTime month = new DateTime(lastDateTime.Value.Year, lastDateTime.Value.Month, 1);
                    this.FromDate = month;
                    this.ToDate = lastDateTime.Value.ToDateTime(TimeOnly.MinValue);
                }
            }

            this.DataGridMessage = "подготовка";
        }

        #region Properties

        public string DataGridMessage { get => this.dataGridMessage; private set => this.SetProperty(ref this.dataGridMessage, value); }

        public override IEnumerable<Shared.PlusPropertyDescriptor> PropertyDescriptors => ModelHelper.GetChangesOfMetersPropertyDescriptors();

        public DateTime? FromDate
        {
            get => this.fromDate;
            set
            {
                if (this.SetProperty(ref this.fromDate, value))
                {
                    this.RaisePropertyChanged(nameof(this.ReportTitle));
                    this.RaisePropertyChanged(nameof(this.DataFilter));
                    this.OnDateChanged();
                }
            }
        }

        public DateTime? ToDate
        {
            get => this.toDate;
            set
            {
                if (this.SetProperty(ref this.toDate, value))
                {
                    this.RaisePropertyChanged(nameof(this.ReportTitle));
                    this.RaisePropertyChanged(nameof(this.DataFilter));
                    this.OnDateChanged();
                }
            }
        }

        public string PersonalAccountForFilter
        {
            get => this.personalAccountForFilter;
            set
            {
                if (this.SetProperty(ref this.personalAccountForFilter, value))
                {
                    this.DoFilter();
                }
            }
        }

        public string AbonentNameForFilter
        {
            get => this.abonentNameForFilter;
            set
            {
                if (this.SetProperty(ref this.abonentNameForFilter, value))
                {
                    this.DoFilter();
                }
            }
        }

        public string OldCounterNumberForFilter
        {
            get => this.oldCounterNumberForFilter;
            set
            {
                if (this.SetProperty(ref this.oldCounterNumberForFilter, value))
                {
                    this.DoFilter();
                }
            }
        }

        public string NewCounterNumberForFilter
        {
            get => this.newCounterNumberForFilter;
            set
            {
                if (this.SetProperty(ref this.newCounterNumberForFilter, value))
                {
                    this.DoFilter();
                }
            }
        }

        public string TPForFilter
        {
            get => this.tPForFilter;
            set
            {
                if (this.SetProperty(ref this.tPForFilter, value))
                {
                    this.DoFilter();
                }
            }
        }

        #region Commands

        public ICommand CommandSetPeriod { get; }

        #endregion

        public override string ReportTitle => $"Сведения по заменам счётчиков за период с {this.FromDate.Value.ToShortDateString()} по {this.ToDate.Value.ToShortDateString()}";

        public override Predicate<ChangeOfMeter> DataFilter => (item) =>
        {
            if (this.FromDate.HasValue && this.ToDate.HasValue && item.ДатаЗамены != default)
            {
                DateTime date = item.ДатаЗамены.ToDateTime(TimeOnly.MinValue);
                return date >= this.FromDate && date <= this.ToDate;
            }
            else
            {
                return false;
            }
        };

        #endregion

        #region Private methods

        protected override ICollectionView BuildAndGetView()
        {
            this.IsBusy = true;
            this.Status = "Подготовка данных ...";
            ICollectionView collectionView = new ListCollectionView(this.Data.ToList());
            using (collectionView.DeferRefresh())
            {
                collectionView.SortDescriptions.Add(new SortDescription(nameof(ChangeOfMeter.ДатаЗамены), ListSortDirection.Ascending));
                collectionView.SortDescriptions.Add(new SortDescription(nameof(ChangeOfMeter.НомерАкта), ListSortDirection.Ascending));
                collectionView.SortDescriptions.Add(new SortDescription(nameof(ChangeOfMeter.НаселённыйПункт), ListSortDirection.Ascending));
                collectionView.SortDescriptions.Add(new SortDescription(nameof(ChangeOfMeter.Фио), ListSortDirection.Ascending));
                collectionView.Filter = this.Filter;
            }

            this.IsBusy = false;
            this.Status = null;

            this.DataGridMessage = DEFAULT_DATA_GRID_MESSAGE;

            return collectionView;
        }

        protected override bool Filter(object obj)
        {
            bool result = true;

            if (obj is not ChangeOfMeter item)
            {
                return false;
            }

            if (this.FromDate.HasValue && this.ToDate.HasValue && item.ДатаЗамены != default)
            {
                DateTime date = item.ДатаЗамены.ToDateTime(TimeOnly.MinValue);
                result = date >= this.FromDate && date <= this.ToDate;
            }

            if (string.IsNullOrWhiteSpace(this.personalAccountForFilter) == false)
            {
                result &= item.Лицевой.ToString(AppSettings.CurrentCulture).Contains(this.personalAccountForFilter, AppSettings.StringComparisonMethod);
            }

            if (string.IsNullOrWhiteSpace(this.abonentNameForFilter) == false && string.IsNullOrWhiteSpace(item.Фио) == false)
            {
                result &= item.Фио.Contains(this.abonentNameForFilter, AppSettings.StringComparisonMethod);
            }

            if (string.IsNullOrWhiteSpace(this.oldCounterNumberForFilter) == false && string.IsNullOrWhiteSpace(item.НомерСнятогоСчетчика) == false)
            {
                result &= item.НомерСнятогоСчетчика.Contains(this.oldCounterNumberForFilter, AppSettings.StringComparisonMethod);
            }

            if (string.IsNullOrWhiteSpace(this.newCounterNumberForFilter) == false && string.IsNullOrWhiteSpace(item.НомерСнятогоСчетчика) == false)
            {
                result &= item.НомерУстановленногоСчетчика.Contains(this.newCounterNumberForFilter, AppSettings.StringComparisonMethod);
            }

            if (string.IsNullOrWhiteSpace(this.tPForFilter) == false && string.IsNullOrWhiteSpace(item.НомерТП) == false)
            {
                result &= item.НомерТП.Contains(this.tPForFilter, AppSettings.StringComparisonMethod);
            }

            return result;
        }

        private void OnDateChanged()
        {
            /*if (View != null)
                View.Refresh();*/
            this.Reset();
        }

        private void DoFilter()
        {
            if (this.View != null)
            {
                this.View.Refresh();
            }
        }
        #endregion

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0453");
            return guid.GetHashCode();
        }
    }
}
