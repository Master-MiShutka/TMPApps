using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ItemsFilter;
using TMP.UI.Controls.WPF;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using Extensions;
    public class FilterViewModel : BaseViewModel
    {
        private ICollectionView _collection;
        private ICollection<string> _filters;

        public FilterViewModel()
        {
            FilterListExtensions.FiltersChanged += FilterListExtensions_FiltersChanged;

            Filters = ModelHelper.MeterPropertiesNames;
        }

        ~FilterViewModel()
        {
            FilterListExtensions.FiltersChanged -= FilterListExtensions_FiltersChanged;
        }

        public ICollection<string> Filters
        {
            get { return _filters; }
            set { if (value.Equals(_filters)) return; _filters = value; RaisePropertyChanged("Filters"); }
        }


        public ICollection<Model.Meter> Meters { get; set; }

        public ICollectionView Collection
        {
            get { return _collection; }
            set {
                if (value.Equals(_collection)) return;
                _collection = value;
                RaisePropertyChanged("Collection");
            }
        }

        public string ActiveFiltersList
        {
            get
            {
                IEnumerable<string> list = FilterListExtensions.Filters == null
                    ? null
                    : FilterListExtensions.Filters.Where(f => (f.Model != null && f.Model.IsActive)).Select(c => c.Key);
                return String.Format("Активные: {0}", String.Join(", ", list)).Replace("_", " ");
            }
        }
        public bool IsAnyFilterActive
        {
            get
            {
                return FilterListExtensions.Filters == null ? false : FilterListExtensions.Filters.Any(f => f.Model == null ? false : f.Model.IsActive);
            }
        }
        public string FilterWindowHeader
        {
            get
            {
                int count = Filters == null ? 0 : FilterListExtensions.Filters.Count(f => f.Model == null ? false : f.Model.IsActive);
                return String.Format("Фильтры ({0})", count == 0 ? string.Empty : count.ToString());
            }
        }

        public ICommand CommandResetFilters =>
            new DelegateCommand(() =>
            {
                if (FilterListExtensions.Filters == null) return;
                foreach (var f in FilterListExtensions.Filters)
                    if (f.Model != null)
                    {
                        f.Model.IsActive = false;
                        foreach (var i in f.Model)
                        {
                            i.IsActive = false;
                        }
                    }
            },
            () => IsAnyFilterActive,
            "Очистить фильтры");

        private void FilterListExtensions_FiltersChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("IsAnyFilterActive");
            RaisePropertyChanged("FilterWindowHeader");
            RaisePropertyChanged("ActiveFiltersList");
        }
    }
}
