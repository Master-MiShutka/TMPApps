namespace ItemsFilter
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using ItemsFilter;
    using ItemsFilter.View;

    public class FilterListExtensions : DependencyObject
    {
        public static IList<FilterControl> FilterControls = new List<FilterControl>();

        public static IList<FilterPresenter> FilterPresenters = new List<FilterPresenter>();

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached("IsActive",
                                         typeof(bool),
                                         typeof(FilterListExtensions),
                                         new PropertyMetadata(false, OnIsActiveChanged));

        public static void SetIsActive(Control element, bool value)
        {
            element.SetValue(IsActiveProperty, value);
        }

        public static bool GetIsActive(Control element)
        {
            return (bool)element.GetValue(IsActiveProperty);
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilterControl filterControl)
            {
                bool newValue = Convert.ToBoolean(e.NewValue);
                bool oldValue = Convert.ToBoolean(e.OldValue);
                if (newValue == false)
                {
                    FilterControls.Remove(filterControl);
                    filterControl.Unloaded -= FilterUnloaded;
                    if (filterControl.ViewModel != null)
                    {
                        filterControl.ViewModel.StateChanged -= ViewModelStateChanged;
                        filterControl.ViewModel.FilterChanged -= ViewModelFilterChanged;
                    }
                }
                else
                {
                    FilterControls.Add(filterControl);
                    filterControl.Loaded += FilterLoaded;
                    filterControl.Unloaded += FilterUnloaded;
                }

                return;
            }

            if (d is IFilterView filterView)
            {
                bool newValue = Convert.ToBoolean(e.NewValue);
                bool oldValue = Convert.ToBoolean(e.OldValue);
                if (newValue == false)
                {
                    filterView.ViewModelChanged -= FilterView_ViewModelChanged;
                    if (filterView.ViewModel != null)
                    {
                        FilterPresenter filterPresenter = filterView.ViewModel.FilterPresenter;
                        FilterPresenters.Remove(filterPresenter);
                        filterPresenter.Filtered -= FilterPresenter_Filter;
                    }
                }
                else
                {
                    filterView.ViewModelChanged += FilterView_ViewModelChanged;
                    if (filterView.ViewModel != null)
                    {
                        FilterPresenter filterPresenter = filterView.ViewModel.FilterPresenter;
                        FilterPresenters.Add(filterPresenter);
                        filterPresenter.Filtered += FilterPresenter_Filter;
                    }
                }
            }
        }

        private static void FilterView_ViewModelChanged(object sender, ViewModelChangedEventArgs e)
        {
            var oldViewModel = e.OldViewModel;
            if (oldViewModel != null)
            {
                FilterPresenter filterPresenter = oldViewModel.FilterPresenter;
                FilterPresenters.Remove(filterPresenter);
                filterPresenter.Filtered -= FilterPresenter_Filter;
            }
            var newViewModel = e.NewViewModel;
            if (newViewModel != null)
            {
                FilterPresenter filterPresenter = newViewModel.FilterPresenter;
                FilterPresenters.Add(filterPresenter);
                filterPresenter.Filtered += FilterPresenter_Filter;
            }
        }

        private static void FilterPresenter_Filter(object sender, FilteredEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void FilterLoaded(object sender, RoutedEventArgs e)
        {
            var filterControl = sender as FilterControl;

            if (filterControl.ViewModel != null)
            {
                filterControl.ViewModel.StateChanged += ViewModelStateChanged;
                filterControl.ViewModel.FilterChanged += ViewModelFilterChanged;
            }
        }

        private static void FilterUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FilterControl filterControl)
            {
                filterControl.Unloaded -= FilterUnloaded;
                if (filterControl.ViewModel != null)
                {
                    filterControl.ViewModel.StateChanged -= ViewModelStateChanged;
                    filterControl.ViewModel.FilterChanged -= ViewModelFilterChanged;
                }
            }
        }

        private static void ViewModelStateChanged(ItemsFilter.ViewModel.FilterControlVm sender, FilterControl.State newValue)
        {
            FiltersStateChanged?.Invoke(sender, new PropertyChangedEventArgs("State"));
        }

        private static void ViewModelFilterChanged(ViewModel.FilterControlVm sender, Model.IFilter filter)
        {
            FiltersChanged?.Invoke(sender, filter);
        }

        public static event PropertyChangedEventHandler FiltersStateChanged;

        public static event ViewModel.FilterControlFilterEventHandler FiltersChanged;
    }
}