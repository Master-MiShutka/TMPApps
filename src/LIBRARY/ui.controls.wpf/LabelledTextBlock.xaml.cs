namespace TMP.UI.WPF.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for LabelledTextBlock.xaml
    /// </summary>
    [TemplatePart(Name = ElementLabel, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementText, Type = typeof(TextBlock))]
    public class LabelledTextBlock : ContentControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";

        private TextBlock label;
        private TextBlock text;

        public LabelledTextBlock()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.label = this.EnforceInstance<TextBlock>(ElementLabel);
            this.text = this.EnforceInstance<TextBlock>(ElementText);
            if (this.text != null & string.IsNullOrEmpty(this.Text) == false)
            {
                this.text.Text = this.Text;
            }
        }

        // Get element from name. If it exist then element instance return, if not, new will be created
        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            T element = this.GetTemplateChild(partName) as T ?? new T();
            return element;
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register("Label",
                typeof(string),
                typeof(LabelledTextBlock),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty TextProperty = DependencyProperty
            .Register("Text",
                typeof(string),
                typeof(LabelledTextBlock),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender, OnTextChanged));

        public static readonly DependencyProperty TextStyleProperty = DependencyProperty
            .Register("TextStyle",
                typeof(Style),
                typeof(LabelledTextBlock),
                new FrameworkPropertyMetadata(null));

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabelledTextBlock me = (LabelledTextBlock)d;
            if (me == null || me.text == null)
            {
                return;
            }

            string value = (string)e.NewValue;
            me.text.Text = value;
        }

        public Style TextStyle
        {
            get => (Style)this.GetValue(TextStyleProperty);
            set => this.SetValue(TextStyleProperty, value);
        }
    }
}
