namespace ItemsFilter.View
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using ItemsFilter.Model;

    /// <summary>
    /// Define View control for IStringFilter model.
    /// </summary>
    [ViewModelView]
    [TemplatePart(Name = StringFilterView.PART_FilterType, Type = typeof(Selector))]
    public class StringFilterView : FilterViewBase<IStringFilter>, IFilterView
    {
        internal const string PART_FilterType = "PART_FilterType";

        static StringFilterView()
        {
            /*DefaultStyleKeyProperty.OverrideMetadata(typeof(StringFilterView),
                new FrameworkPropertyMetadata(typeof(StringFilterView)));*/
        }

        /// <summary>
        /// Instance of a selector allowing to choose the filtering mode
        /// </summary>
        private ItemsControl selectorFilterType;

        /// <summary>
        /// Create new instance of StringFilterView.
        /// </summary>
        public StringFilterView() : base()
        {
        }

        /// <summary>
        /// Create new instance of StringFilterView and accept IStringFilter ViewModel.
        /// </summary>
        /// <param name="viewModel"></param>
        public StringFilterView(object viewModel)
        {
            base.ViewModel = viewModel as IStringFilter;
        }

        /// <summary>
        /// Called when the control template is applied to this control
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.selectorFilterType = this.GetTemplateChild(PART_FilterType) as ItemsControl;
            if (this.selectorFilterType != null)
            {
                this.selectorFilterType.ItemsSource = this.GetFilterModes();
            }
        }

        private IEnumerable<StringFilterMode> GetFilterModes()
        {
            foreach (var item in typeof(StringFilterMode).GetEnumValues())
            {
                yield return (StringFilterMode)item;
            }
        }
    }
}
