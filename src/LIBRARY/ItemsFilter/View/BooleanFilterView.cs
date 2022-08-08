namespace ItemsFilter.View
{
    using ItemsFilter.Model;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Define View control for IBooleanFilter model.
    /// </summary>
    [ViewModelView]
    [TemplatePart(Name = PART_Input, Type = typeof(CheckBox))]
    public class BooleanFilterView : FilterViewBase<IBooleanFilter>
    {
        private const string PART_Input = "PART_Input";

        static BooleanFilterView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BooleanFilterView),
                new FrameworkPropertyMetadata(typeof(BooleanFilterView)));
        }

        private CheckBox checkBox;

        /// <summary>
        /// Create new instance of BooleanFilterView.
        /// </summary>
        public BooleanFilterView()
        {
        }

        /// <summary>
        /// Create new instance of IBooleanFilter and accept model.
        /// </summary>
        /// <param name="model">IBooleanFilter model</param>
        public BooleanFilterView(object model)
            : this()
        {
            this.ViewModel = model as IBooleanFilter;
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this.checkBox != null)
            {
                this.checkBox.RemoveHandler(CheckBox.CheckedEvent, new RoutedEventHandler(this.CheckBox_Checked));
            }

            base.OnApplyTemplate();
            this.checkBox = this.GetTemplateChild(PART_Input) as CheckBox;
            if (this.checkBox != null)
            {
                this.checkBox.IsThreeState = true;

                this.checkBox.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(this.CheckBox_Checked), true);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateIsCheckedSource((CheckBox)sender);
        }

        private static void UpdateIsCheckedSource(CheckBox chb)
        {
            chb.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        }
    }
}
