namespace ItemsFilter.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using ItemsFilter.Model;

    public delegate void FilterControlStateChangedEventHandler(FilterControlVm sender, FilterControl.State newValue);

    public delegate void FilterControlFilterEventHandler(FilterControlVm sender, IFilter filter);

    /// <summary>
    /// View model for <c>FilterControl</c>.
    /// FilterControlVm is IEnumerable&lt;Filter&gt;.
    /// An instance of FilterControlVm not created directly. Instead, the procedure FilterPresenter.TryGetFilterControlVm(string viewKey, IEnumerable&lt;FilterInitializer&gt; filterInitializers)
    /// prepares a model for FilterControl, as IEnumerable&lt;Filter&gt;, where each Filter instance bound to FilterControlVm for the transmission of changes.
    /// Usually, instance of FilterControlVm created by FilterControl when it need this and disposed on raise Unload event.
    /// </summary>
    public class FilterControlVm : ObservableCollection<IFilter>, IDisposable
    {
        private readonly object lockFlag = new object();
        private bool isDisposed = false;
        private bool isActive;
        private bool isOpen;
        private bool isEnable;
        private FilterControl.State state;

        internal FilterControlVm()
        {
        }

        /// <summary>
        /// Get FilterControl state.
        /// </summary>
        public FilterControl.State State
        {
            get => this.state;

            private set
            {
                if (this.state != value)
                {
                    this.state = value;
                    if (this.StateChanged != null)
                    {
                        lock (this.StateChanged)
                        {
                            this.StateChanged(this, this.state);
                        }
                    }

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.State)));
                }
            }
        }

        /// <summary>
        /// Provide FilterControlStateChanged event.
        /// </summary>
        public event FilterControlStateChangedEventHandler StateChanged;

        public event FilterControlFilterEventHandler FilterChanged;

        /// <summary>
        /// Get or set FilterControl.IsEnabled.
        /// </summary>
        public bool IsEnable
        {
            get => this.isEnable;

            set
            {
                if (this.isEnable != value)
                {
                    this.isEnable = value;
                    if (value)
                    {
                        this.State |= FilterControl.State.Enable;
                    }
                    else
                    {
                        this.State &= ~FilterControl.State.Enable;
                    }

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsEnable)));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the popup is currently displaying on the screen.
        /// </summary>
        public bool IsOpen
        {
            get => this.isOpen;

            set
            {
                if (this.isOpen != value)
                {
                    this.isOpen = value;
                    if (value)
                    {
                        this.State |= FilterControl.State.Open;
                    }
                    else
                    {
                        this.State &= ~FilterControl.State.Open;
                    }

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsOpen)));
                }
            }
        }

        /// <summary>
        /// Get or set whether filter is active.
        /// </summary>
        public bool IsActive
        {
            get => this.isActive;

            set
            {
                if (this.isActive != value)
                {
                    this.isActive = value;
                    if (value)
                    {
                        this.State |= FilterControl.State.Active;
                    }
                    else
                    {
                        this.State &= ~FilterControl.State.Active;
                    }

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsActive)));
                }
            }
        }

        /// <summary>
        /// Gets whether Dispose has been called.
        /// </summary>
        public bool IsDisposed => this.isDisposed;

        // Summary:
        //     Inserts a filter into the collection at the specified index and attach filter to FilterControlVm.
        //
        // Parameters:
        //   index:
        //     The zero-based index at which item should be inserted.
        //
        //   item:
        //     The object to insert.
        protected override void InsertItem(int index, IFilter filter)
        {
            if (!this.isDisposed)
            {
                base.InsertItem(index, filter);
                filter.Attach(this);
                this.OnFilterStateChanged();
            }
        }

        // Summary:
        //    Detach FilterControlVm from all filters and remove all filters from collection.
        protected override void ClearItems()
        {
            foreach (var item in base.Items)
            {
                item.Detach(this);
            }

            this.OnFilterStateChanged();
            base.ClearItems();
        }

        // Summary:
        //     Replaces the filter at the specified index.
        //     Detach FilterControlVm from removed filter and attach FilterControlVm to new filter.
        //
        // Parameters:
        //   index:
        //     The zero-based index of the element to replace.
        //
        //   item:
        //     The new value for the element at the specified index.
        protected override void SetItem(int index, IFilter filter)
        {
            if (!this.isDisposed)
            {
                base[index].Detach(this);
                base.SetItem(index, filter);
                filter.Attach(this);
                this.OnFilterStateChanged();
            }
        }

        // Summary:
        //     Moves the filter at the specified index to a new location in the collection.
        //
        // Parameters:
        //   oldIndex:
        //     The zero-based index specifying the location of the item to be moved.
        //
        //   newIndex:
        //     The zero-based index specifying the new location of the item.
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (!this.isDisposed)
            {
                base.MoveItem(oldIndex, newIndex);
            }
        }

        // Summary:
        //     Detach FilterControlVm from filter at the specified index of the collection and removes filter.
        //
        // Parameters:
        //   index:
        //     The zero-based index of the element to remove.
        protected override void RemoveItem(int index)
        {
            if (!this.isDisposed)
            {
                base[index].Detach(this);
                base.RemoveItem(index);
                this.OnFilterStateChanged();
            }
        }

        /// <summary>
        /// Detach FilterControlVm from all filters and remove all filters from collection.
        /// </summary>
        public void Dispose()
        {
            lock (this.lockFlag)
            {
                if (!this.isDisposed)
                {
                    this.isDisposed = true;
                    this.Clear();
                }
            }
        }

        internal void OnFilterStateChanged()
        {
            bool active = false;
            foreach (var item in base.Items)
            {
                active |= item.IsActive;
            }

            this.IsActive = active;
        }

        internal void OnFilterChanged(IFilter filter)
        {
            if (filter != null && this.FilterChanged != null)
            {
                lock (this.FilterChanged)
                {
                    this.FilterChanged(this, filter);
                }
            }
        }
    }
}