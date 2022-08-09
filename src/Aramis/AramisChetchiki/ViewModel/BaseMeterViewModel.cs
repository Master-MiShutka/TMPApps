namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Input;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;

    /// <summary>
    /// Базовая модель представления данных коллекции счетчиков
    /// </summary>
    public class BaseMeterViewModel : BaseDataViewModel<Meter>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool notShowDeleted = true;
        private bool notShowDisconnected = true;

        public BaseMeterViewModel()
        {
            this.Data = MainViewModel?.Data?.Meters;

            this.Init();
        }

        public BaseMeterViewModel(IEnumerable<Meter> meters = null)
            : base(meters)
        {
            if (meters == null)
            {
                this.Data = MainViewModel?.Data?.Meters;
            }

            this.Init();
        }

        #region Public properties

        #region Info

        /// <summary>
        /// Количество уникальных номеров счётчиков
        /// </summary>
        public int MetersCount => this.Data == null ? 0 : this.Data.DistinctBy(meter => meter.НомерСчетчика).Count();

        /// <summary>
        /// Количество л/с
        /// </summary>
        public int AccountsCount => this.Data == null ? 0 : this.Data.DistinctBy(meter => meter.Лицевой).Count();

        /// <summary>
        /// Количество абонентов
        /// </summary>
        public int AbonentsCount => this.Data == null ? 0 : this.Data.DistinctBy(meter => meter.БазовыйЛицевой).Count();

        #endregion

        /// <summary>
        /// Команда для сокрытия удаленных счетчиков
        /// </summary>
        public ICommand CommandNotShowDeleted { get; private set; }

        /// <summary>
        /// Команда для сокрытия отключенных счетчиков
        /// </summary>
        public ICommand CommandNotShowDisconnected { get; private set; }

        #endregion

        #region Overrides

        protected override ICollectionView BuildAndGetView()
        {
            ICollectionView collectionView = this.GetCollectionView(this.Data);

            if (collectionView.IsEmpty)
                return collectionView;

            using (collectionView.DeferRefresh())
            {
                collectionView.SortDescriptions.Add(new SortDescription(nameof(Meter.Адрес), ListSortDirection.Ascending));

                collectionView.Filter = this.Filter;
            }

            this.FilterPresenter = ItemsFilter.FiltersManager.TryGetFilterPresenter(collectionView);

            WindowWithDialogs.DispatcherExtensions.InUi(() =>
            {
                // добавление фильтров
                string[] filterFields =
                {
                    nameof(Meter.Лицевой),
                    nameof(Meter.НаселённыйПунктИУлицаСНомеромДома),
                    nameof(Meter.ФиоСокращ),
                    nameof(Meter.НомерСчетчика),
                    nameof(Meter.ТП),
                };
                foreach (string filterField in filterFields)
                {
                    this.Filters.Add((ItemsFilter.Model.IStringFilter)this.FilterPresenter.TryGetFilter(filterField, new ItemsFilter.Initializer.StringFilterInitializer()));
                }
            });

            this.UpdateShowDeletedFilter();
            this.UpdateShowDisconnectedFilter();
            
            (collectionView as INotifyPropertyChanged).PropertyChanged += this.View_PropertyChanged;

            return collectionView;
        }

        public override int GetHashCode()
        {
            System.Guid guid = new("3E70130E-CE09-40CC-88ED-AF06915976C8");
            return guid.GetHashCode();
        }

        #endregion

        #region Private methods
        
        private void UpdateShowDeletedFilter()
        {
            ItemsFilter.Model.IBooleanFilter filter = (ItemsFilter.Model.IBooleanFilter)this.FilterPresenter.TryGetFilter(nameof(Meter.Удалён), new ItemsFilter.Initializer.BooleanFilterInitializer());
            if (filter != null)
            {
                filter.Value = !this.notShowDeleted;
            }
        }

        private void UpdateShowDisconnectedFilter()
        {
            ItemsFilter.Model.IBooleanFilter filter = (ItemsFilter.Model.IBooleanFilter)this.FilterPresenter.TryGetFilter(nameof(Meter.Отключён), new ItemsFilter.Initializer.BooleanFilterInitializer());
            if (filter != null)
            {
                filter.Value = !this.notShowDisconnected;
            }
        }        

        private void Init()
        {
            logger.Info("Init.");

            this.CommandNotShowDeleted = new DelegateCommand(() =>
            {
                this.IsBusy = true;
                this.Status = "обновление ...";
                
                this.notShowDeleted = !this.notShowDeleted;

                this.UpdateShowDeletedFilter();

                /*this.RaisePropertyChanged(nameof(this.DataFilter));
                this.RaisePropertyChanged(nameof(this.Data));
                this.RaisePropertyChanged(nameof(this.View));
                this.RaisePropertyChanged(nameof(this.ItemsCount));
                this.RaisePropertyChanged(nameof(this.ItemsCount));*/

                this.IsBusy = false;
            },
            () => this.Data != null);

            this.CommandNotShowDisconnected = new DelegateCommand(() =>
            {
                this.IsBusy = true;
                this.Status = "обновление ...";

                this.notShowDisconnected = !this.notShowDisconnected;

                this.UpdateShowDisconnectedFilter();

                /*this.RaisePropertyChanged(nameof(this.DataFilter));
                this.RaisePropertyChanged(nameof(this.Data));
                this.RaisePropertyChanged(nameof(this.View));
                this.RaisePropertyChanged(nameof(this.ItemsCount));
                this.RaisePropertyChanged(nameof(this.ItemsCount));*/

                this.IsBusy = false;
            },
            () => this.Data != null);
        }

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();

            logger.Info("Data updated.");

            (this.CommandNotShowDeleted as DelegateCommand)?.RaiseCanExecuteChanged();
            (this.CommandNotShowDisconnected as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        private void View_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Count" && sender is CollectionView collectionView)
            {
                this.CollectionViewItemsCount = collectionView.Count;
                this.RaisePropertyChanged(propertyName: nameof(this.PercentOfTotal));
            }
        }

        #endregion
    }
}
