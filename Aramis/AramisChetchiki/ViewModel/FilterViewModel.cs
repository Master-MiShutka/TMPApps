namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Input;
    using ItemsFilter;
    using TMP.Shared;
    using TMP.Shared.Commands;

    public class FilterViewModel : BaseViewModel
    {
        private ICollectionView collectionView;
        private ICollection<PlusPropertyDescriptor> properties;
        private ICollectionView propertiesCollection;

        public FilterViewModel()
        {
            FilterListExtensions.FiltersStateChanged += this.FilterListExtensions_FiltersChanged;

            this.properties = ModelHelper.MeterPropertiesCollection.Values;

            this.propertiesCollection = (CollectionView)CollectionViewSource.GetDefaultView(this.properties);
            if (this.propertiesCollection.CanGroup == true && this.propertiesCollection.GroupDescriptions.Count == 0)
            {
                PropertyGroupDescription groupDescription
                    = new("GroupName");
                this.propertiesCollection.GroupDescriptions.Add(groupDescription);
            }

            this.RaisePropertyChanged(propertyName: nameof(this.ActiveFiltersList));
            this.RaisePropertyChanged(propertyName: nameof(this.IsAnyFilterActive));
            this.RaisePropertyChanged(propertyName: nameof(this.FilterWindowHeader));
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            FilterListExtensions.FiltersStateChanged -= this.FilterListExtensions_FiltersChanged;
        }

        public ICollectionView PropertiesCollection => this.propertiesCollection;

        public ICollectionView CollectionView
        {
            get => this.collectionView;
            set
            {
                if (ReferenceEquals(value, this.collectionView))
                {
                    return;
                }

                this.collectionView = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.CollectionViewItemsCount));
            }
        }

        public int CollectionViewItemsCount => (this.CollectionView as ListCollectionView) == null ? 0 : (this.CollectionView as ListCollectionView).Count;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ToDo")]
        public string ActiveFiltersList
        {
            get
            {
                IEnumerable<string> list = FilterListExtensions.FilterControls?.Where(f => f.ViewModel != null && f.ViewModel.IsActive).Select(c => c.Key);
                return $"Активные: {string.Join(", ", list)}".Replace("_", " ", StringComparison.Ordinal);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ToDo")]
        public bool IsAnyFilterActive => FilterListExtensions.FilterControls != null && FilterListExtensions.FilterControls.Any(f => f.ViewModel != null && f.ViewModel.IsActive);

        public string FilterWindowHeader
        {
            get
            {
                int count = this.PropertiesCollection == null ? 0 : FilterListExtensions.FilterControls.Count(f => f.ViewModel != null && f.ViewModel.IsActive);
                return string.Format(AppSettings.CurrentCulture, "Фильтры ({0})", count == 0 ? string.Empty : count);
            }
        }

        public ICommand CommandResetFilters =>
            new DelegateCommand(
                () =>
            {
                if (FilterListExtensions.FilterControls == null)
                {
                    return;
                }

                foreach (FilterControl f in FilterListExtensions.FilterControls)
                {
                    if (f.ViewModel != null)
                    {
                        f.ViewModel.IsActive = false;
                        this.RaisePropertyChanged(propertyName: nameof(this.ActiveFiltersList));
                        this.RaisePropertyChanged(propertyName: nameof(this.IsAnyFilterActive));
                        this.RaisePropertyChanged(propertyName: nameof(this.FilterWindowHeader));
                        foreach (ItemsFilter.Model.IFilter i in f.ViewModel)
                        {
                            i.IsActive = false;
                        }
                    }
                }
            },
                () => this.IsAnyFilterActive);

        private void FilterListExtensions_FiltersChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(propertyName: nameof(this.IsAnyFilterActive));
            this.RaisePropertyChanged(propertyName: nameof(this.FilterWindowHeader));
            this.RaisePropertyChanged(propertyName: nameof(this.ActiveFiltersList));
        }
    }
}
