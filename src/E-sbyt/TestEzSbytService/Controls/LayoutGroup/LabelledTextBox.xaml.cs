using System.Windows;
using System.Windows.Controls;

namespace TMP.Work.AmperM.TestApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для LabelledTextBox.xaml
    /// </summary>
    [TemplatePart(Name = ElementLabel, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementText, Type = typeof(TextBox))]
    public partial class LabelledTextBox : UserControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";

        private TextBlock _label;
        private TextBox _text;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _label = EnforceInstance<TextBlock>(ElementLabel);
            _text = EnforceInstance<TextBox>(ElementText);

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
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty TextProperty = DependencyProperty
            .Register("Text",
                    typeof(string),
                    typeof(LabelledTextBox),
                    new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MultiLineProperty = DependencyProperty
        .Register("MultiLine",
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false, OnMultiLineChanged));

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty
        .Register("AcceptsReturn",
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty
        .Register("AcceptsTab",
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty
            .Register("Watermark",
                    typeof(string),
                    typeof(LabelledTextBox),
                    new FrameworkPropertyMetadata(default(string)));
        public static readonly DependencyProperty IsWaitingForDataProperty = DependencyProperty
        .Register("IsWaitingForData",
                typeof(bool),
                typeof(LabelledTextBox),
                new FrameworkPropertyMetadata(false));

        public LabelledTextBox()
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
        public bool MultiLine
        {
            get { return (bool)GetValue(MultiLineProperty); }
            set { SetValue(MultiLineProperty, value); }
        }
        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }
        public bool AcceptsTab
        {
            get { return (bool)GetValue(AcceptsTabProperty); }
            set { SetValue(AcceptsTabProperty, value); }
        }
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }
        public bool IsWaitingForData
        {
            get { return (bool)GetValue(IsWaitingForDataProperty); }
            set { SetValue(IsWaitingForDataProperty, value); }
        }

        private static void OnMultiLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabelledTextBox me = (LabelledTextBox)d;
            if (me == null) return;
            if (me._text == null) return;
            bool value = (bool)e.NewValue;
            if (value)
            {
                me._text.TextWrapping = TextWrapping.Wrap;
                me._text.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                me._text.TextWrapping = TextWrapping.NoWrap;
                me._text.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
    }
}