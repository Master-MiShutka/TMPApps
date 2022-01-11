namespace TMP.UI.Controls.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for LabelledTextBox.xaml
    /// </summary>
    [TemplatePart(Name = ElementLabel, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementText, Type = typeof(TextBox))]
    public class LabelledTextBox : ContentControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";

        private TextBlock label;
        private TextBox text;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.label = this.EnforceInstance<TextBlock>(ElementLabel);
            this.text = this.EnforceInstance<TextBox>(ElementText);
        }

        // Get element from name. If it exist then element instance return, if not, new will be created
        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            T element = this.GetTemplateChild(partName) as T ?? new T();
            return element;
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register(nameof(Label),
                typeof(string),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty TextProperty = DependencyProperty
            .Register(nameof(Text),
                    typeof(string),
                    typeof(LabelledTextBox),
                    new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MultiLineProperty = DependencyProperty
            .Register(nameof(MultiLine),
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false, OnMultiLineChanged));

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty
            .Register(nameof(AcceptsReturn),
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty
            .Register(nameof(AcceptsTab),
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsReadonlyProperty = DependencyProperty
            .Register(nameof(IsReadonly),
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty TextStyleProperty = DependencyProperty
            .Register(nameof(TextStyle),
                typeof(Style),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ValidationErrorProperty = DependencyProperty
            .Register(nameof(ValidationError),
                typeof(RoutedEvent),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(default(RoutedEvent)));

        public LabelledTextBox()
        {
        }

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

        public bool MultiLine
        {
            get => (bool)this.GetValue(MultiLineProperty);
            set => this.SetValue(MultiLineProperty, value);
        }

        public bool AcceptsReturn
        {
            get => (bool)this.GetValue(AcceptsReturnProperty);
            set => this.SetValue(AcceptsReturnProperty, value);
        }

        public bool AcceptsTab
        {
            get => (bool)this.GetValue(AcceptsTabProperty);
            set => this.SetValue(AcceptsTabProperty, value);
        }

        public bool IsReadonly
        {
            get => (bool)this.GetValue(IsReadonlyProperty);
            set => this.SetValue(IsReadonlyProperty, value);
        }

        public Style TextStyle
        {
            get => (Style)this.GetValue(TextStyleProperty);
            set => this.SetValue(TextStyleProperty, value);
        }

        public RoutedEvent ValidationError
        {
            get => (RoutedEvent)this.GetValue(ValidationErrorProperty);
            set => this.SetValue(ValidationErrorProperty, value);
        }

        private static void OnMultiLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabelledTextBox me = (LabelledTextBox)d;
            if (me == null)
            {
                return;
            }

            if (me.text == null)
            {
                return;
            }

            bool value = (bool)e.NewValue;
            if (value)
            {
                me.text.TextWrapping = TextWrapping.Wrap;
                me.text.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                me.text.TextWrapping = TextWrapping.NoWrap;
                me.text.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
    }
}
