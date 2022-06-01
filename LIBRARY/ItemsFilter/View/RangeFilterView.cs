namespace ItemsFilter.View
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using ItemsFilter.Model;

    /// <summary>
    /// Define View control for IRangeFilter model.
    /// </summary>
    [ViewModelView]
    [TemplatePart(Name = PART_From, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_To, Type = typeof(TextBox))]
    public class RangeFilterView : FilterViewBase<IRangeFilter>
    {
        private const string PART_From = "PART_From";
        private const string PART_To = "PART_To";

        static RangeFilterView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeFilterView),
                new FrameworkPropertyMetadata(typeof(RangeFilterView)));
        }

        private TextBox txtBoxFrom;
        private TextBox textBoxTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeFilterView"/> class.
        /// </summary>
        public RangeFilterView() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeFilterView"/> class and accept ViewModel.
        /// </summary>
        /// <param name="viewModel">IRangeFilter ViewModel</param>
        public RangeFilterView(object viewModel)
        {
            base.ViewModel = viewModel as IRangeFilter;
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this.txtBoxFrom != null)
            {
                this.txtBoxFrom.RemoveHandler(TextBox.KeyDownEvent, new KeyEventHandler(this.TextBox_KeyDown));
                this.txtBoxFrom.RemoveHandler(TextBox.LostFocusEvent, new RoutedEventHandler(this.TextBox_LostFocus));
            }

            if (this.textBoxTo != null)
            {
                this.textBoxTo.RemoveHandler(TextBox.KeyDownEvent, new KeyEventHandler(this.TextBox_KeyDown));
                this.textBoxTo.RemoveHandler(TextBox.LostFocusEvent, new RoutedEventHandler(this.TextBox_LostFocus));
            }

            base.OnApplyTemplate();
            this.textBoxTo = this.GetTemplateChild(PART_To) as TextBox;
            this.txtBoxFrom = this.GetTemplateChild(PART_From) as TextBox;
            if (this.txtBoxFrom != null)
            {
                this.txtBoxFrom.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(this.TextBox_KeyDown), true);
                this.txtBoxFrom.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler(this.TextBox_LostFocus), true);
            }

            if (this.textBoxTo != null)
            {
                this.textBoxTo.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(this.TextBox_KeyDown), true);
                this.textBoxTo.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler(this.TextBox_LostFocus), true);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxSource((TextBox)sender);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        UpdateTextBoxSource((TextBox)sender);
                        e.Handled = true;
                        return;
                    }
            }
        }

        private static void UpdateTextBoxSource(TextBox tb)
        {
            tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
