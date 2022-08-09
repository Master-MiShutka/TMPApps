﻿namespace ItemsFilter
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using ItemsFilter.Initializer;
    using ItemsFilter.Model;
    using ItemsFilter.View;
    using ItemsFilter.ViewModel;

    /// <summary>
    /// Common Filter control that represent collection of Filters in UI.
    /// </summary>
    [TemplateVisualState(GroupName = "FilterState", Name = "Disable")]
    [TemplateVisualState(GroupName = "FilterState", Name = "Enable")]
    [TemplateVisualState(GroupName = "FilterState", Name = "Active")]
    [TemplateVisualState(GroupName = "FilterState", Name = "Open")]
    [TemplateVisualState(GroupName = "FilterState", Name = "OpenActive")]
    public partial class FilterControl : ItemsControl
    {
        /// <summary>
        /// Visual states of FilterControl.
        /// </summary>
        [Flags]
        public enum State
        {
            /// <summary>
            /// FilterControl is disable.
            /// </summary>
            Disable = 0,

            /// <summary>
            /// FilterControl is enable.
            /// </summary>
            Enable = 1,

            /// <summary>
            /// DropDown part is show.
            /// </summary>
            Open = 2,

            /// <summary>
            /// Filter is active.
            /// </summary>
            Active = 4,
            OpenActive = 6,
        }

        private static bool isInDesignMode = (bool)System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                                System.ComponentModel.DesignerProperties.IsInDesignModeProperty,
                                typeof(System.Windows.DependencyObject))
                                .Metadata.DefaultValue;

        static FilterControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterControl),
                new FrameworkPropertyMetadata(typeof(FilterControl)));
        }

        private bool isLoaded;

        /// <summary>
        /// Instance of FilterPresenter that attached to FilterControl.
        /// </summary>
        protected FilterPresenter filterPresenter;

        /// <summary>
        /// Create new instance of FilterControl.
        /// </summary>
        public FilterControl()
        {
            this.Loaded += this.FilterControl_Loaded;
            this.Unloaded += this.FilterControl_Unloaded;
        }

        #region dp object ParentCollection

        /// <summary>
        /// Parent Dependency Property
        /// </summary>
        public static readonly DependencyProperty ParentCollectionProperty =
            DependencyProperty.Register("ParentCollection", typeof(IEnumerable), typeof(FilterControl),
                new FrameworkPropertyMetadata((object)null,
                    new PropertyChangedCallback(OnParentCollectionChanged)));

        /// <summary>
        /// Gets or sets the source collection for filtering.
        /// May be ICollectionView or any IEnumerable.
        /// If Parent is ICollectionView, FilterControl bind to Parent itself;
        /// otherwise, FilterControl bind to default collection view for the given source collection.
        /// </summary>
        public IEnumerable ParentCollection
        {
            get => (IEnumerable)this.GetValue(ParentCollectionProperty);

            set => this.SetValue(ParentCollectionProperty, value);
        }

        /// <summary>
        /// Handles changes to the Parent property.
        /// </summary>
        private static void OnParentCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilterControl target = (FilterControl)d;
            IEnumerable oldParent = (IEnumerable)e.OldValue;
            IEnumerable newParent = (IEnumerable)e.NewValue;
            target.OnParentCollectionChanged(oldParent, newParent);
        }

        /// <summary>
        /// Trying to load a model and provides derived classes an opportunity to handle changes to the Parent property.
        /// </summary>
        protected virtual void OnParentCollectionChanged(IEnumerable oldParent, IEnumerable newParent)
        {
            if (this.isLoaded)
            {
                this.LoadViewModel();
            }
        }

        #endregion
        #region Key

        /// <summary>
        /// Key Dependency Property
        /// </summary>
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(FilterControl),
                new FrameworkPropertyMetadata((string)null,
                    new PropertyChangedCallback(OnKeyChanged)));

        /// <summary>
        /// Gets or sets the key for the identification filter (the filter set) in the parent collection view filters.
        /// The key and initializers from FilterInitializersManager define a set of filters that ItemsPresenter provides into a view model for the  FilterControl.
        /// If multiple instances of the Filter Control have the same key and FilterInitializer in FilterInitializersManager,
        /// then these instances  will display the same filter in the view model.
        /// Derived classes can give an additional meaning to the value of the properties Key.
        /// For example, the class ColumnFilter uses key value as name of the item property.
        /// </summary>
        public string Key
        {
            get => (string)this.GetValue(KeyProperty);

            set => this.SetValue(KeyProperty, value);
        }

        /// <summary>
        /// Handles changes to the Key property.
        /// </summary>
        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilterControl target = (FilterControl)d;
            if (!isInDesignMode && target.ViewModel != null)
            {
                throw new InvalidOperationException("Change Key after FilterControl was attached to FilterPresenter not allowed.");
            }

            string oldKey = (string)e.OldValue;
            string newKey = (string)e.NewValue;
            target.OnKeyChanged(oldKey, newKey);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Key property.
        /// </summary>
        protected virtual void OnKeyChanged(string oldKey, string newKey)
        {
        }

        #endregion
        #region FilterInitializersManager

        /// <summary>
        /// FilterInitializersManager Dependency Property
        /// </summary>
        public static readonly DependencyProperty FilterInitializersManagerProperty =
            DependencyProperty.Register("FilterInitializersManager", typeof(FilterInitializersManager), typeof(FilterControl),
                new FrameworkPropertyMetadata((FilterInitializersManager)null,
                    new PropertyChangedCallback(OnFilterInitializersManagerChanged)));

        /// <summary>
        /// Gets or sets the FilterInitializersManager.
        /// FilterInitializersManager contains a set of initializers determines the composition of the filters in the view model.
        /// If FIlterInitializersManager is null, it used default FilterInitialisersManager that provided by FilterInitializersManager.Default static property.
        /// FilterInitializersManager.Default included EqualFilterInitializer, LessOrEqualFilterInitializer, GreaterOrEqualFilterInitializer,
        ///                                         RangeFilterInitializer(), StringFilterInitializer(), EnumFilterInitializer().
        /// </summary>
        public FilterInitializersManager FilterInitializersManager
        {
            get => (FilterInitializersManager)this.GetValue(FilterInitializersManagerProperty);

            set => this.SetValue(FilterInitializersManagerProperty, value);
        }

        /// <summary>
        /// Handles changes to the FilterInitializersManager property.
        /// </summary>
        private static void OnFilterInitializersManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilterControl target = (FilterControl)d;
            if (!isInDesignMode && target.ViewModel != null)
            {
                throw new InvalidOperationException("Change Key after FilterControl was attached to FilterPresenter not allowed.");
            }

            FilterInitializersManager oldFilterInitializersManager = (FilterInitializersManager)e.OldValue;
            FilterInitializersManager newFilterInitializersManager = (FilterInitializersManager)e.NewValue;
            target.OnFilterInitializersManagerChanged(oldFilterInitializersManager, newFilterInitializersManager);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the FilterInitializersManager property.
        /// </summary>
        protected virtual void OnFilterInitializersManagerChanged(FilterInitializersManager oldFilterInitializersManager, FilterInitializersManager newFilterInitializersManager)
        {
        }

        #endregion
        #region ViewModel

        /// <summary>
        /// Model Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey ViewModelPropertyKey
            = DependencyProperty.RegisterReadOnly("ViewModel", typeof(FilterControlVm), typeof(FilterControl),
                new FrameworkPropertyMetadata((FilterControlVm)null,
                    new PropertyChangedCallback(OnViewModelChanged)));

        public static readonly DependencyProperty ViewModelProperty
            = ViewModelPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the FilterControlVm.
        /// </summary>
        /// <return>Instance of FilterControlVm if FilterControl was successfully attached to ParentCollection, otherwise null.</return>
        public FilterControlVm ViewModel => (FilterControlVm)this.GetValue(ViewModelProperty);

        /// <summary>
        /// Handles changes to the Model property.
        /// </summary>
        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilterControl target = (FilterControl)d;

            FilterControlVm oldViewModel = (FilterControlVm)e.OldValue;
            if (oldViewModel != null)
            {
                oldViewModel.Dispose();
                oldViewModel.StateChanged -= target.OnStateChanged;
                target.ItemsSource = null;
            }

            FilterControlVm newViewModel = target.ViewModel;
            if (newViewModel != null)
            {
                newViewModel.StateChanged += target.OnStateChanged;
                target.ItemsSource = newViewModel;
                target.SetVisualState(newViewModel.State);
            }

            target.OnViewModelChanged(newViewModel);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Model property.
        /// </summary>
        protected virtual void OnViewModelChanged(FilterControlVm newModel)
        {
        }

        #endregion
        private void LoadViewModel()
        {
            this.SetValue(ViewModelPropertyKey, this.CreateViewModel());
        }

        /// <summary>
        /// Create model for FilterControl, if it is possible in current state.
        /// </summary>
        /// <returns>Instance of FilterControlVm or null.</returns>
        protected virtual FilterControlVm CreateViewModel()
        {
            FilterControlVm vm = null;
            this.filterPresenter = this.Parent == null ? null : FiltersManager.TryGetFilterPresenter(this.ParentCollection);
            if (this.filterPresenter != null)
            {
                vm = this.filterPresenter.TryGetFilterControlVm(this.Key, this.FilterInitializersManager);
            }

            return vm;
        }

        // Summary:
        //     Prepares the specified element to display the specified item.
        //
        // Parameters:
        //   element:
        //     Element used to display the specified item.
        //
        //   item:
        //     Specified item.
        protected override void PrepareContainerForItemOverride(DependencyObject container, object item)
        {
            base.PrepareContainerForItemOverride(container, item);
            if (item != null && container is ContentPresenter)
            {
                Type itemType = item.GetType();
                object[] viewAttr = itemType.GetCustomAttributes(typeof(ViewAttribute), true);
                if (viewAttr.Length > 0)
                {
                    UIElement view = null;
                    try
                    {
                        Type viewType = ((ViewAttribute)viewAttr[0]).ViewType;
                        if (viewType.GetCustomAttributes(typeof(ViewModelViewAttribute), true).Length > 0)

                        // Ok, make new view using constructor ...FilterView(model).
                        {
                            if (viewType.IsGenericType)
                            {
                                Type[] typeArgs = itemType.GetGenericArguments();
                                Type constructed = viewType.MakeGenericType(typeArgs);
                                view = (UIElement)Activator.CreateInstance(constructed, item);
                            }
                            else
                            {
                                view = (UIElement)Activator.CreateInstance(viewType, item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        int identLevel = Debug.IndentLevel;
                        do
                        {
                            Debug.WriteLine(ex.Message);
                            ex = ex.InnerException;
                            Debug.Indent();
                        }
                        while (ex != null);
                        Debug.IndentLevel = identLevel;
                    }

                    ((ContentPresenter)container).Content = view;
                }
                else
                {
                    ;
                }
            }
        }

        private void FilterControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.isLoaded = true;
            this.LoadViewModel();
        }

        private void FilterControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SetValue(ViewModelPropertyKey, null);
            this.isLoaded = false;
        }

        private void OnStateChanged(FilterControlVm sender, State filterState)
        {
            this.SetVisualState(filterState);
        }

        private void SetVisualState(State filterState)
        {
            if (filterState == State.Disable)
            {
                VisualStateManager.GoToState(this, filterState.ToString(), false);
            }
            else if ((filterState & State.OpenActive) == State.OpenActive)
            {
                VisualStateManager.GoToState(this, State.OpenActive.ToString(), false);
            }
            else if (filterState.HasFlag(State.Open))
            {
                VisualStateManager.GoToState(this, State.Open.ToString(), false);
            }
            else if (filterState.HasFlag(State.Active))
            {
                VisualStateManager.GoToState(this, State.Active.ToString(), false);
            }
            else if (filterState.HasFlag(State.Enable))
            {
                VisualStateManager.GoToState(this, State.Enable.ToString(), false);
            }
            else
            {
                VisualStateManager.GoToState(this, filterState.ToString(), true);
            }
        }
    }
}