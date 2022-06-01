namespace ItemsFilter.View
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using ItemsFilter.Model;

    /// <summary>
    /// Define View control for IComparableFilter model.
    /// </summary>
    [ViewModelView]
    [TemplatePart(Name = PART_Input, Type = typeof(TextBox))]
    public class ComparableFilterView : FilterViewBase<IComparableFilter>
    {
        private const string PART_Input = "PART_Input";

        static ComparableFilterView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComparableFilterView),
                new FrameworkPropertyMetadata(typeof(ComparableFilterView)));
        }

        private TextBox textBox;

        /// <summary>
        /// Create new instance of ComparableFilterView.
        /// </summary>
        public ComparableFilterView()
        {
        }

        /// <summary>
        /// Create new instance of ComparableFilterView and accept model.
        /// </summary>
        /// <param name="model">IComparableFilter model</param>
        public ComparableFilterView(object model)
            : this()
        {
            base.ViewModel = model as IComparableFilter;
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this.textBox != null)
            {
                this.textBox.RemoveHandler(TextBox.KeyDownEvent, new KeyEventHandler(this.TextBox_KeyDown));
                this.textBox.RemoveHandler(TextBox.LostFocusEvent, new RoutedEventHandler(this.TextBox_LostFocus));
            }

            base.OnApplyTemplate();
            this.textBox = this.GetTemplateChild(PART_Input) as TextBox;
            if (this.textBox != null)
            {
                this.textBox.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(this.TextBox_KeyDown), true);
                this.textBox.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler(this.TextBox_LostFocus), true);
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
