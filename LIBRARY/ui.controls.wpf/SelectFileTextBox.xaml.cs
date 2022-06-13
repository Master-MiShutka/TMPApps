namespace TMP.UI.Controls.WPF
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using TMP.Shared.Commands;
    using WindowsNative;

    /// <summary>
    /// Interaction logic for SelectFolderTextBox.xaml
    /// </summary>
    [TemplatePart(Name = ElementLabel, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementText, Type = typeof(TextBox))]
    [TemplatePart(Name = ElementButton, Type = typeof(Button))]
    public class SelectFileTextBox : ContentControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";
        private const string ElementButton = "PART_Select_File_Button";

        private TextBlock label;
        private TextBox text;
        private Button button;

        public SelectFileTextBox()
        {
            this.DataContext = this;

            this.SelectFile = new DelegateCommand(() =>
            {
                IntPtr parentHWnd = new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle;
                Filter[] filters = WindowsNative.Filter.ParseWindowsFormsFilter(this.Filter);

                string selection = FileOpenDialog.ShowSingleSelectDialog(parentHWnd, this.DialogTitle,
                    initialDirectory: null, defaultFileName: this.DefaultExt, filters: filters, selectedFilterZeroBasedIndex: 0);

                if (selection != null)
                {
                    this.SelectedFile = selection;
                }
            });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.label = this.EnforceInstance<TextBlock>(ElementLabel);
            this.text = this.EnforceInstance<TextBox>(ElementText);
            this.button = this.EnforceInstance<Button>(ElementButton);
            if (this.button.Command == null)
            {
                this.button.Command = this.SelectFile;
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
        .Register(nameof(Label),
                typeof(string),
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty
        .Register(nameof(DialogTitle),
                typeof(string),
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata("Select file"));

        public static readonly DependencyProperty FilterProperty = DependencyProperty
        .Register(nameof(Filter),
                typeof(string),
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata("*.*"));

        public static readonly DependencyProperty DefaultExtProperty = DependencyProperty
        .Register(nameof(DefaultExt),
                typeof(string),
                typeof(SelectFileTextBox),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedFileProperty = DependencyProperty
            .Register(nameof(SelectedFile),
                    typeof(string),
                    typeof(SelectFileTextBox),
                    new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedFileChanged));

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        public string DialogTitle
        {
            get => (string)this.GetValue(DialogTitleProperty);
            set => this.SetValue(DialogTitleProperty, value);
        }

        public string Filter
        {
            get => (string)this.GetValue(FilterProperty);
            set => this.SetValue(FilterProperty, value);
        }

        public string DefaultExt
        {
            get => (string)this.GetValue(DefaultExtProperty);
            set => this.SetValue(DefaultExtProperty, value);
        }

        public string SelectedFile
        {
            get => (string)this.GetValue(SelectedFileProperty);
            set => this.SetValue(SelectedFileProperty, value);
        }

        private static void OnSelectedFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null)
            {
                return;
            }

            SelectFileTextBox sftb = d as SelectFileTextBox;
            if (sftb == null || sftb.text == null)
            {
                return;
            }

            sftb.text.Text = e.NewValue.ToString();
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
                {
                    return new ValidationResult(false, "Введены неверные данные");
                }

                if (!System.IO.File.Exists((string)value))
                {
                    return new ValidationResult(false, "Файл не найден");
                }
            }
            catch (Exception)
            {
                return new ValidationResult(false, "Неверный путь");
            }

            return new ValidationResult(true, null);
        }
    }
}
