namespace ItemsFilter.View
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using ItemsFilter.Model;

    /// <summary>
    /// Provide base class for filter View that include Filter as Model property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TemplatePart(Name = FilterViewBase<T>.PART_Name, Type = typeof(TextBlock))]
    public abstract class FilterViewBase<T> : Control, IFilterView
    {
        public const string PART_Name = "PART_Name";

        static FilterViewBase()
        {
            CommandManager.RegisterClassCommandBinding(typeof(FilterViewBase<T>),
                new CommandBinding(FilterCommand.Clear, ClearFilterExecute, ClearFilterCanExecute));
        }

        private static void ClearFilterCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((FilterViewBase<T>)sender).ViewModel != null && ((IFilter)((FilterViewBase<T>)sender).ViewModel).IsActive;
        }

        private static void ClearFilterExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ((IFilter)((FilterViewBase<T>)sender).ViewModel).IsActive = false;
        }

        private TextBlock txtName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewBase&lt;T&gt;"/> class.
        /// </summary>
        public FilterViewBase()
        {
        }

        #region ViewModel

        /// <summary>
        /// ViewModel Dependency Property
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(T), typeof(FilterViewBase<T>),
                new FrameworkPropertyMetadata(default(T),
                    new PropertyChangedCallback(OnViewModelChanged)));

        /// <summary>
        /// Gets or sets the ViewModel.
        /// </summary>
        public T ViewModel
        {
            get => (T)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Handles changes to the VievModel.
        /// </summary>
        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilterViewBase<T> target = (FilterViewBase<T>)d;
            T oldViewModel = (T)e.OldValue;
            T newViewModel = (T)e.NewValue;
            target.OnViewModelChanged(oldViewModel, newViewModel);

            target.ViewModelChanged?.Invoke(target, new ViewModelChangedEventArgs(oldViewModel as IFilter, newViewModel as IFilter));
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ViewModel property.
        /// </summary>
        protected virtual void OnViewModelChanged(T oldViewModel, T newViewModel)
        {
        }

        public event ViewModelChangedEventHandler ViewModelChanged;

        #endregion

        IFilter IFilterView.ViewModel => (IFilter)this.ViewModel;

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.txtName = this.GetTemplateChild(PART_Name) as TextBlock;
            this.InitializeBindings();
        }

        /// <summary>
        /// Initializes the bindings.
        /// </summary>
        private void InitializeBindings()
        {
            if (this.txtName != null)
            {
                this.txtName.SetBinding(TextBlock.TextProperty, new Binding("ViewModel.Name")
                {
                    Mode = BindingMode.OneWay,
                    Source = this,
                });
            }
        }
    }
}
