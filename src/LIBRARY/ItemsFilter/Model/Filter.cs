namespace ItemsFilter.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using ItemsFilter.ViewModel;

    /// <summary>
    /// Base class for a filter.
    /// </summary>
    public abstract class Filter : IFilter, INotifyPropertyChanged
    {
        private string name = ItemsFilter.Resources.Strings.FilterText;
        private bool isActive;
        private FilterPresenter filterPresenter;
        private readonly List<FilterControlVm> attachedFilterControlVmodels = new List<FilterControlVm>();

        /// <summary>
        /// Get attached FilterPresenter.
        /// </summary>
        public FilterPresenter FilterPresenter => this.filterPresenter;

        /// <summary>
        /// Represent action that determine is item match filter.
        /// </summary>
        /// <param name="sender">FilterPresenter that contains filter.</param>
        /// <param name="e">FilterEventArgs include Item and Accepted fields.</param>
        public abstract void IsMatch(FilterPresenter sender, FilterEventArgs e);

        /// <summary>
        /// Get or set Name of filter.
        /// </summary>
        public string Name
        {
            get => this.name;

            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.RaisePropertyChanged(nameof(this.Name));
                }
            }
        }

        /// <summary>
        /// Get or set value, determines is filter IsMatch action include in parentCollection filter.
        /// </summary>
        public bool IsActive
        {
            get => this.isActive;

            set
            {
                if (this.isActive != value)
                {
                    this.isActive = value;
                    IDisposable defer = this.FilterPresenter?.DeferRefresh();
                    this.OnIsActiveChanged();
                    if (defer != null)
                    {
                        defer.Dispose();
                    }

                    this.RaisePropertyChanged(nameof(this.IsActive));
                }
            }
        }

        /// <summary>
        /// Provides class handling for the AttachPresenter event that occurs when FilterPresenter is attached.
        /// </summary>
        protected virtual void OnAttachPresenter(FilterPresenter presenter)
        {
        }

        /// <summary>
        /// Provides class handling for the DetachPresenter event that occurs when FilterPresenter is detached.
        /// </summary>
        protected virtual void OnDetachPresenter(FilterPresenter presenter)
        {
        }

        /// <summary>
        /// Provide derived class IsActiveChanged event.
        /// </summary>
        protected virtual void OnIsActiveChanged()
        {
            this.RaiseFilterStateChanged();
        }

        /// <summary>
        /// Report attached listeners that filter changed.
        /// </summary>
        protected void RaiseFilterStateChanged()
        {
            if (this.filterPresenter != null)
            {
                this.filterPresenter.ReceiveFilterChanged(this);
            }

            foreach (var vm in this.attachedFilterControlVmodels)
            {
                vm.OnFilterStateChanged();
            }
        }

        /// <summary>
        /// Report attached listeners that filter changed.
        /// </summary>
        protected void RaiseFilterChanged()
        {
            foreach (var vm in this.attachedFilterControlVmodels)
            {
                vm.OnFilterChanged(this);
            }
        }

        /// <summary>
        /// Number attached to filter instances FilterControlVm.
        /// </summary>
        public int CountAttachedFilterControls => this.attachedFilterControlVmodels.Count;

        public void Attach(FilterControlVm vm)
        {
            if (!this.attachedFilterControlVmodels.Contains(vm))
            {
                this.attachedFilterControlVmodels.Add(vm);
            }
        }

        public void Detach(FilterPresenter presenter)
        {
            if (presenter != null)
            {
                presenter.Filter -= this.IsMatch;
            }

            if (presenter == this.filterPresenter)
            {
                this.filterPresenter = null;
            }

            this.OnDetachPresenter(presenter);
        }

        public void Attach(FilterPresenter presenter)
        {
            this.filterPresenter = presenter;
            if (this.filterPresenter != null)
            {
                this.filterPresenter.ReceiveFilterChanged(this);
            }

            this.OnAttachPresenter(presenter);
        }

        public void Detach(FilterControlVm vm)
        {
            this.attachedFilterControlVmodels.Remove(vm);
        }

        #region Члены INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        protected void VerifyPropertyName(string propertyName)
        {
            var myType = this.GetType();

#if NETFX_CORE
            if (!string.IsNullOrEmpty(propertyName)
                && myType.GetTypeInfo().GetDeclaredProperty(propertyName) == null)
            {
                throw new ArgumentException("Property not found", propertyName);
            }
#else
            if (!string.IsNullOrEmpty(propertyName)
                && myType.GetProperty(propertyName) == null)
            {
#if !SILVERLIGHT
                var descriptor = this as ICustomTypeDescriptor;

                if (descriptor != null)
                {
                    if (descriptor.GetProperties()
                        .Cast<PropertyDescriptor>()
                        .Any(property => property.Name == propertyName))
                    {
                        return;
                    }
                }
#endif

                throw new ArgumentException("Property not found", propertyName);
            }
#endif
        }

        #endregion Члены INotifyPropertyChanged
    }
}