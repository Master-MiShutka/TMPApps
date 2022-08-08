namespace ItemsFilter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using ItemsFilter.Initializer;
    using ItemsFilter.Model;
    using ItemsFilter.ViewModel;

    // <summary>
    // Выполняет роль управляющего для создания экземпляров фильтра и их подключения к CollectionView
    // </summary>
    public sealed class FilterPresenter : DependencyObject
    {

        private ReadOnlyCollection<ItemPropertyInfo> itemProperties;
        private int itemsDeferRefreshCount = 0;
        private IDisposable itemsDeferRefresh = null;
        private Predicate<object> filterFunction;
        private bool isFilterActive;
        private readonly ICollectionView collectionView;
        private readonly Dictionary<string, FiltersCollection> filters;

        private event FilterEventHandler FilterEventHandler;

        private readonly FilteredEventArgs filteredEventArgs;

        internal FilterPresenter(ICollectionView source)
        {
            this.collectionView = source;
            this.filteredEventArgs = new FilteredEventArgs(source);
            this.itemProperties = (IItemProperties)source == null
                ? new ReadOnlyCollection<ItemPropertyInfo>(new ItemPropertyInfo[] { })
                : ((IItemProperties)source).ItemProperties;

            if (this.itemProperties == null)
                this.itemProperties = new ReadOnlyCollection<ItemPropertyInfo>(new ItemPropertyInfo[] { });

            this.filterFunction = new Predicate<object>(this.FilterFunction);
            this.filters = new Dictionary<string, FiltersCollection>();
        }

        // <summary>
        // Возвращает подключенную коллекцию
        // </summary>
        public ICollectionView CollectionView => this.collectionView;

        /// <summary>
        /// Get or set a value that indicates whether the defined filter set to attached ItemsControl.Items.PropertyFilter.
        /// </summary>
        public bool IsFilterActive
        {
            get => this.isFilterActive;

            set
            {
                if (this.isFilterActive != value)
                {
                    this.isFilterActive = value;
                    this.DeferRefresh().Dispose();
                }
            }
        }

        // <summary>
        // Initializes and configures the ViewModel for FilterControl.
        // </summary>
        // <param name="viewKey">A string representing the key for a set of filters.</param>
        // <param name="filterInitializers"> Filter initialisers to determine permissible set of the filters in the FilterControlVm.</param>
        // <returns>Instance of FilterControlVm that was bind to view.</returns>
        public FilterControlVm TryGetFilterControlVm(string viewKey, IEnumerable<FilterInitializer> filterInitializers)
        {
            // string viewKey = view.Key;
            FilterControlVm viewModel = null;
            if (viewKey != null)
            {
                FiltersCollection filtersEntry;

                // Get registered collection by key.
                if (this.filters.ContainsKey(viewKey))
                {
                    filtersEntry = this.filters[viewKey];
                }
                else
                {
                    filtersEntry = new FiltersCollection(this);
                    this.filters.Add(viewKey, filtersEntry);
                }

                filterInitializers = filterInitializers ?? FilterInitializersManager.Default;

                foreach (FilterInitializer initializer in filterInitializers)
                {
                    Type filterKey = initializer.GetType();
                    IFilter filter;
                    if (filtersEntry.ContainsKey(filterKey))
                    {
                        filter = filtersEntry[filterKey];
                    }
                    else
                    {
                        filter = initializer.NewFilter(this, viewKey);
                        if (filter != null)
                        {
                            filtersEntry[filterKey] = filter;
                        }
                    }

                    if (filter != null)
                    {
                        viewModel = viewModel ?? new FilterControlVm();
                        viewModel.Add(filter);
                    }
                }

                // view.ItemsSource = viewModel;
            }

            return viewModel;
        }

        /// <summary>
        /// Retrieves  or tries to create the filter model, using as a key pair {viewKey, initializer}.
        /// </summary>
        /// <param name="viewKey">A string representing a key of the set of filters.</param>
        /// <param name="initializer">Initialiser filter that defines filter in the collection of filters.</param>
        /// <returns>FilterPresenter instance, if it is possible provide for couples viewKey and initializer. Otherwise, null.</returns>
        public IFilter TryGetFilter(string viewKey, FilterInitializer initializer)
        {
            IFilter filter = null;
            if (viewKey != null)
            {
                FiltersCollection filtersEntry;

                // Get registered collection by key.
                if (this.filters.ContainsKey(viewKey))
                {
                    filtersEntry = this.filters[viewKey];
                }
                else
                {
                    filtersEntry = new FiltersCollection(this);
                    this.filters.Add(viewKey, filtersEntry);
                }

                Type filterKey = initializer.GetType();
                if (filtersEntry.ContainsKey(filterKey))
                {
                    filter = filtersEntry[filterKey];
                }
                else
                {
                    filter = initializer.NewFilter(this, viewKey);
                    if (filter != null)
                    {
                        filtersEntry[filterKey] = filter;
                    }
                }
            }

            return filter;
        }

        // Represent a set of Predicate<Object> that used to generate filter function.
        internal event FilterEventHandler Filter
        {
            add
            {
                if (this.filterFunction == null)
                {
                    this.filterFunction = new Predicate<object>(this.FilterFunction);
                }

                var deferRefresh = this.DeferRefresh();
                this.FilterEventHandler += value;
                this.IsFilterActive = true;
                deferRefresh.Dispose();
            }

            remove
            {
                var deferRefresh = this.DeferRefresh();
                this.FilterEventHandler -= value;

                // if (itemsControl != null && _Filter==null)
                //    itemsControl.Items.PropertyFilter = null;
                this.IsFilterActive = this.FilterEventHandler != null;
                if (this.FilterEventHandler == null)
                {
                    this.filterFunction = null;
                }

                deferRefresh.Dispose();
            }
        }

        /// <summary>
        ///  Enters a defer cycle that you can use to change filter of the view and delay automatic refresh.
        /// </summary>
        /// <returns> An System.IDisposable object that you can use to dispose of the calling object. </returns>
        public IDisposable DeferRefresh()
        {
            return new DisposeItemsDeferRefresh(this);
        }

        /// <summary>
        /// Возвращает коллекцию, содержащую информацию о свойствах элементов коллекции
        /// </summary>
        public ReadOnlyCollection<ItemPropertyInfo> ItemProperties
        {
            get => this.itemProperties;

            private set
            {
                if (this.itemProperties != value)
                {
                    this.itemProperties = value;
                }
            }
        }

        /// <summary>
        /// Возникает после фильтрации, вызванной изменением условий фильтров
        /// </summary>
        public EventHandler<FilteredEventArgs> Filtered;

        // Сообщает FilterPresenter об изменении состояния фильтра.
        // Для экземпляра фильтра в активном состоянии, производится включение фильтра в условие фильтрации представления коллекции.
        // Для экземпляра фильтра в пассивном состоянии, производится исключение фильтра из условия фильтрации коллекции.

        /// <summary>
        /// Receives notice of the change filter conditions and IsActive property.
        /// </summary>
        /// <param name="filter"></param>
        internal void ReceiveFilterChanged(IFilter filter)
        {
            var defer = this.DeferRefresh();
            this.Filter -= filter.IsMatch;
            if (filter.IsActive)
            {
                this.Filter += filter.IsMatch;
            }

            defer.Dispose();
        }

        private void RaiseFiltered()
        {
            lock (this.filteredEventArgs)
            {
                this.Filtered?.Invoke(this, this.filteredEventArgs);
            }
        }

        private bool FilterFunction(object obj)
        {
            if (this.FilterEventHandler != null)
            {
                FilterEventArgs args = new FilterEventArgs(obj);
                this.FilterEventHandler(this, args);
                return args.Accepted;
            }
            else
            {
                return true;
            }
        }

        private class DisposeItemsDeferRefresh : IDisposable
        {
            private readonly FilterPresenter filterPresenter;
            private bool isDisposed = false;

            internal DisposeItemsDeferRefresh(FilterPresenter filterVm)
            {
                this.filterPresenter = filterVm;
                if (this.filterPresenter.CollectionView is IEditableCollectionView cv)
                {
                    if (cv.IsAddingNew)
                    {
                        cv.CommitNew();
                    }

                    if (cv.IsEditingItem)
                    {
                        cv.CommitEdit();
                    }
                }

                if (this.filterPresenter.itemsDeferRefreshCount == 0)
                {
                    this.filterPresenter.itemsDeferRefresh = this.filterPresenter.CollectionView.DeferRefresh();
                }

                this.filterPresenter.itemsDeferRefreshCount++;
            }

            public void Dispose()
            {
                if (!this.isDisposed)
                {
                    this.filterPresenter.itemsDeferRefreshCount--;
                    if (this.filterPresenter.itemsDeferRefreshCount <= 0)
                    {
                        this.filterPresenter.itemsDeferRefreshCount = 0;
                        if (this.filterPresenter.CollectionView is IEditableCollectionView cv)
                        {
                            if (cv.IsAddingNew)
                            {
                                cv.CancelNew();
                            }

                            if (cv.IsEditingItem)
                            {
                                cv.CancelEdit();
                            }
                        }

                        if (this.filterPresenter.isFilterActive)
                        {
                            this.filterPresenter.CollectionView.Filter = this.filterPresenter.filterFunction;
                        }
                        else
                            if (this.filterPresenter.CollectionView.Filter != null)
                        {
                            this.filterPresenter.CollectionView.Filter = null;
                        }

                        this.filterPresenter.RaiseFiltered();
                        if (this.filterPresenter.itemsDeferRefresh != null)
                        {
                            this.filterPresenter.itemsDeferRefresh.Dispose();
                        }

                        this.filterPresenter.itemsDeferRefresh = null;
                    }

                    this.isDisposed = true;
                }
                else
                {
                    throw new ObjectDisposedException("FilterPresenter(" + this.filterPresenter.CollectionView.ToString() + ").GetDeferRefresh()");
                }
            }
        }
    }
}