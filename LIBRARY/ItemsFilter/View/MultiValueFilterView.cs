namespace ItemsFilter.View
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using ItemsFilter.Model;

    /// <summary>
    /// Define View control for IMultiValueFilter model.
    /// </summary>
    [ViewModelView]
    [TemplatePart(Name = MultiValueFilterView.PART_ItemsTemplateName, Type = typeof(System.Windows.Controls.Primitives.Selector))]
    public class MultiValueFilterView : FilterViewBase<IMultiValueFilter>
    {
        public const string PART_ItemsTemplateName = "PART_Items";

        static MultiValueFilterView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiValueFilterView),
                new FrameworkPropertyMetadata(typeof(MultiValueFilterView)));
        }

        private bool isModelAttached;
        private ItemsControl itemsCtrl;

        private IList SelectedItems { get; set; }

        /// <summary>
        /// Create new instance of MultiValueFilterView;
        /// </summary>
        public MultiValueFilterView()
        {
            this.Unloaded += this.MultiValueFilterView_Unloaded;
            this.Loaded += this.MultiValueFilterView_Loaded;
        }

        /// <summary>
        /// Create new instance of MultiValueFilterView and accept viewModel.
        /// </summary>
        /// <param name="viewModel">IMultiValueFilter viewModel</param>
        public MultiValueFilterView(object viewModel) : this()
        {
            base.ViewModel = viewModel as IMultiValueFilter;
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ViewModel property.
        /// </summary>
        protected override void OnViewModelChanged(IMultiValueFilter oldViewModel, IMultiValueFilter newViewModel)
        {
            this.DetachViewModel(this.itemsCtrl, oldViewModel);
            this.AttachViewModel(this.itemsCtrl, newViewModel);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            this.DetachViewModel(this.itemsCtrl, this.ViewModel);
            base.OnApplyTemplate();
            this.itemsCtrl = this.GetTemplateChild(MultiValueFilterView.PART_ItemsTemplateName) as ItemsControl;

            this.SelectedItems = this.itemsCtrl is ListBox listBox ? listBox.SelectedItems : (this.itemsCtrl as System.Windows.Controls.Primitives.MultiSelector)?.SelectedItems;

            this.AttachViewModel(this.itemsCtrl, this.ViewModel);
        }

        private void MultiValueFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            this.AttachViewModel(this.itemsCtrl, this.ViewModel);
        }

        private void MultiValueFilterView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DetachViewModel(this.itemsCtrl, this.ViewModel);
        }

        private void SubscribeSelectionChanged(IMultiValueFilter viewModel)
        {
            if (this.itemsCtrl is ListBox listBox)
            {
                listBox.SelectionChanged += viewModel.SelectedValuesChanged;
            }
            if (this.itemsCtrl is System.Windows.Controls.Primitives.MultiSelector multiSelector)
            {
                multiSelector.SelectionChanged += viewModel.SelectedValuesChanged;
            }
        }

        private void UnSubscribeSelectionChanged(IMultiValueFilter viewModel)
        {
            if (this.itemsCtrl is ListBox listBox)
            {
                listBox.SelectionChanged -= viewModel.SelectedValuesChanged;
            }
            if (this.itemsCtrl is System.Windows.Controls.Primitives.MultiSelector multiSelector)
            {
                multiSelector.SelectionChanged -= viewModel.SelectedValuesChanged;
            }
        }

        private void AttachViewModel(ItemsControl itemsCtrl, IMultiValueFilter newViewModel)
        {
            if (!this.isModelAttached && this.itemsCtrl != null && newViewModel != null)
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                {
                    var enumerator = newViewModel.AvailableValues.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        this.SelectedItems.Add(enumerator.Current);
                    }

                    return;
                }

                IList selectedItems = this.SelectedItems;
                selectedItems.Clear();
                foreach (var item in newViewModel.SelectedValues)
                {
                    selectedItems.Add(item);
                }

                this.SubscribeSelectionChanged(newViewModel);

                ((INotifyCollectionChanged)newViewModel.SelectedValues).CollectionChanged += this.MultiValueFilterView_CollectionChanged;

                this.isModelAttached = true;
            }
        }

        private void MultiValueFilterView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UnSubscribeSelectionChanged(this.ViewModel);

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.SelectedItems.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        int itemIndex = this.SelectedItems.IndexOf(item);
                        if (itemIndex >= 0)
                        {
                            this.SelectedItems.RemoveAt(itemIndex);
                        }
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        int itemIndex = this.SelectedItems.IndexOf(item);
                        if (itemIndex < 0)
                        {
                            this.SelectedItems.Add(item);
                        }
                    }
                }
            }

            this.SubscribeSelectionChanged(this.ViewModel);
        }

        private void DetachViewModel(ItemsControl itemsCtrl, IMultiValueFilter oldViewModel)
        {
            if (this.isModelAttached && this.itemsCtrl != null && oldViewModel != null)
            {
                ((INotifyCollectionChanged)oldViewModel.SelectedValues).CollectionChanged -= this.MultiValueFilterView_CollectionChanged;

                this.UnSubscribeSelectionChanged(oldViewModel);

                this.isModelAttached = false;
            }
        }
    }
}
