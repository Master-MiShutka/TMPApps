using System;
using System.Windows;
using System.Windows.Controls;

namespace TMP.UI.Controls.WPF
{
    /// <summary>
    /// Interaction logic for LabelledTextBlock.xaml
    /// </summary>
    [TemplatePart(Name = ElementLabel, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementText, Type = typeof(TextBlock))]
    public partial class LabelledTextBlock : UserControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";

        private TextBlock _label;
        private TextBlock _text;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _label = EnforceInstance<TextBlock>(ElementLabel);
            _text = EnforceInstance<TextBlock>(ElementText);
        }
        //Get element from name. If it exist then element instance return, if not, new will be created
        T EnforceInstance<T>(string partName) where T : FrameworkElement, new()
        {
            T element = GetTemplateChild(partName) as T ?? new T();
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
                    new FrameworkPropertyMetadata(default(string), OnTextChanged));

        public LabelledTextBlock()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabelledTextBlock me = (LabelledTextBlock)d;
            if (me == null || me._text == null) return;
            string value = (string)e.NewValue;
            me._text.Text = value;
        }
    }

}
