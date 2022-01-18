﻿namespace TMP.WORK.AramisChetchiki.ViewModel
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
        private bool notShowDeleted = true;

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
        /// Команда для сокрытия удаленных элементов из коллекции
        /// </summary>
        public ICommand CommandNotShowDeleted { get; private set; }

        #endregion

        #region Overrides

        protected override ICollectionView BuildAndGetView()
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Data);
            using (collectionView.DeferRefresh())
            {
                collectionView.SortDescriptions.Add(new SortDescription(nameof(Meter.Адрес), ListSortDirection.Ascending));

                collectionView.Filter = this.Filter;
            }

            this.FilterPresenter = ItemsFilter.FiltersManager.TryGetFilterPresenter(collectionView);

            TMPApplication.DispatcherExtensions.InUi(() =>
            {
                // добавление фильтров
                string[] filterFields = { nameof(Meter.Лицевой), nameof(Meter.НаселённыйПунктИУлицаСНомеромДома), nameof(Meter.ФиоСокращ), nameof(Meter.НомерСчетчика), nameof(Meter.ТП) };
                foreach (var filterField in filterFields)
                {
                    this.Filters.Add((ItemsFilter.Model.IStringFilter)this.FilterPresenter.TryGetFilter(filterField, new ItemsFilter.Initializer.StringFilterInitializer()));
                }
            });

            (collectionView as INotifyPropertyChanged).PropertyChanged += this.View_PropertyChanged;

            return collectionView;
        }

        public override Predicate<Meter> DataFilter => (meter) =>
        {
            if (meter != null & this.notShowDeleted && meter.Удалён == true)
                return false;
            else
                return true;
        };

        #endregion

        #region Private methods

        private void Init()
        {
            this.CommandNotShowDeleted = new DelegateCommand(() =>
            {
                this.IsBusy = true;
                this.Status = "обновление ...";

                this.notShowDeleted = !this.notShowDeleted;
                this.RaisePropertyChanged(nameof(this.DataFilter));
                if (this.View != null)
                {
                    this.View.Refresh();
                }
                //this.RaisePropertyChanged(nameof(this.Data));
                //this.RaisePropertyChanged(nameof(this.View));

                this.IsBusy = false;
            },
            (o) => this.Data != null);
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