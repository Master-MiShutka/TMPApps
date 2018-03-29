using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TMP.UI.Controls.WPF
{
    /// <summary>
    /// Interaction logic for SelectFolderTextBox.xaml
    /// </summary>
    [TemplatePart(Name = ElementLabel, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementText, Type = typeof(TextBox))]
    [TemplatePart(Name = ElementButton, Type = typeof(Button))]
    public partial class SelectFileTextBox : UserControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";
        private const string ElementButton = "PART_Select_File_Button";

        private TextBlock _label;
        private TextBox _text;
        private Button _button;

        public SelectFileTextBox()
        {
            InitializeComponent();
            Root.DataContext = this;

            SelectFile = new DelegateCommand(() =>
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                dialog.Title = DialogTitle;
                dialog.Filter = Filter;
                dialog.DefaultExt = DefaultExt;
                var result = dialog.ShowDialog(Application.Current.MainWindow);
                if (result != null && result.HasValue)
                {
                    SelectedFile = dialog.FileName;
                }
            });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _label = EnforceInstance<TextBlock>(ElementLabel);
            _text = EnforceInstance<TextBox>(ElementText);
            _button = EnforceInstance<Button>(ElementButton);
            if (_button.Command == null)
                _button.Command = SelectFile;


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
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty
        .Register("DialogTitle",
                typeof(string),
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata("Select file"));

        public static readonly DependencyProperty FilterProperty = DependencyProperty
        .Register("Filter",
                typeof(string),
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata("*.*"));

        public static readonly DependencyProperty DefaultExtProperty = DependencyProperty
        .Register("DefaultExt",
                typeof(string),
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedFileProperty = DependencyProperty
            .Register("SelectedFile",
                    typeof(string),
                    typeof(SelectFileTextBox),
                    new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedFileChanged));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public string DialogTitle
        {
            get { return (string)GetValue(DialogTitleProperty); }
            set { SetValue(DialogTitleProperty, value); }
        }
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        public string DefaultExt
        {
            get { return (string)GetValue(DefaultExtProperty); }
            set { SetValue(DefaultExtProperty, value); }
        }

        public string SelectedFile
        {
            get { return (string)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        private static void SelectedFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null) return;
            SelectFileTextBox sftb = d as SelectFileTextBox;
            if (sftb == null || sftb._text == null) return;
            sftb._text.Text = e.NewValue.ToString();
        }

        public ICommand SelectFile { get; private set; }
    }

    public class FileExistsRule : ValidationRule
    {
        public static FileExistsRule Instance = new FileExistsRule();

        public override ValidationResult Validate(object value,
               System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                if (!(value is string))
                    return new ValidationResult(false, "Введены неверные данные");

                if (!System.IO.File.Exists((string)value))
                    return new ValidationResult(false, "Файл не найден");
            }
            catch (Exception)
            {
                return new ValidationResult(false, "Неверный путь");
            }
            return new ValidationResult(true, null);
        }
    }
}
