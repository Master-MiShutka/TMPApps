using System;
using System.Collections.Generic;
using System.Windows;
using ItemsFilter;
using System.ComponentModel;

namespace ItemsFilter
{
    public class FilterListExtensions : DependencyObject
    {
        public static IList<FilterControl> Filters = new List<FilterControl>();
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached("IsActive",
                                         typeof(bool),
                                         typeof(FilterListExtensions),
                                         new PropertyMetadata(false, OnIsActiveChanged));

        public static void SetIsActive(FilterControl element, bool value)
        {
            element.SetValue(IsActiveProperty, value);
        }

        public static bool GetIsActive(FilterControl element)
        {
            return (bool)element.GetValue(IsActiveProperty);
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var filter = d as FilterControl;

            if (filter != null)
            {
                bool newValue = Convert.ToBoolean(e.NewValue);
                bool oldValue = Convert.ToBoolean(e.OldValue);
                if (newValue == false)
                {
                    Filters.Remove(filter);
                    filter.Unloaded -= Filter_Unloaded;
                    if (filter.Model != null)
                        filter.Model.StateChanged -= Model_StateChanged;
                }
                else
                {
                    Filters.Add(filter);

                    filter.Loaded += Filter_Loaded;

                    filter.Unloaded += Filter_Unloaded;
                }
            }
        }

        private static void Filter_Loaded(object sender, RoutedEventArgs e)
        {
            var filter = sender as FilterControl;

            if (filter.Model != null)
                filter.Model.StateChanged += Model_StateChanged;

        }

        private static void Filter_Unloaded(object sender, RoutedEventArgs e)
        {
            FilterControl filter = sender as FilterControl;
            if (filter != null)
            {
                filter.Unloaded -= Filter_Unloaded;
                if (filter.Model != null)
                    filter.Model.StateChanged -= Model_StateChanged;
            }
        }

        private static void Model_StateChanged(ItemsFilter.ViewModel.FilterControlVm sender, FilterControl.State newValue)
        {
            var handler = FiltersChanged;
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs("State"));
            }
        }

        public static event PropertyChangedEventHandler FiltersChanged;
    }
}