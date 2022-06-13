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
    public class SelectFolderTextBox : ContentControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";
        private const string ElementButton = "PART_Select_Folder_Button";

        private TextBlock label;
        private TextBox text;
        private Button button;

        public SelectFolderTextBox()
        {
            this.SelectFolder = new DelegateCommand(() =>
            {
                this.OnStartSelectFolder?.Invoke(this, EventArgs.Empty);

                string selection = FolderBrowserDialog.ShowDialog(Application.Current.MainWindow, "Выберите папку", null);
                if (selection != null)
                {
                    this.SelectedPath = selection;
                }

                this.OnEndSelectFolder?.Invoke(this, EventArgs.Empty);
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
                this.button.Command = this.SelectFolder;
            }
        }

        // Get element from name. If it exist then element instance return, if not, new will be created
        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            T element = this.GetTemplateChild(partName) as T ?? new T();
            return element;
        }

        public event EventHandler OnStartSelectFolder;

        public event EventHandler OnEndSelectFolder;

        public static readonly DependencyProperty LabelProperty = DependencyProperty
        .Register(nameof(Label),
                typeof(string),
                typeof(SelectFolderTextBox),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty
            .Register(nameof(SelectedPath),
                    typeof(string),
                    typeof(SelectFolderTextBox),
                    new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedPathChanged));

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        public string SelectedPath
        {
            get => (string)this.GetValue(SelectedPathProperty);
            set => this.SetValue(SelectedPathProperty, value);
        }

        private static void SelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null)
            {
                return;
            }

            SelectFolderTextBox sftb = d as SelectFolderTextBox;
            if (sftb == null || sftb.text == null)
            {
                return;
            }

            sftb.text.Text = e.NewValue == null ? string.Empty : e.NewValue.ToString();
        }

        public ICommand SelectFolder { get; private set; }
    }

    public class DirectoryExistsRule : ValidationRule
    {
        public override ValidationResult Validate(object value,
               System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                if (value == null || string.IsNullOrWhiteSpace(value as string))
                {
                    return ValidationResult.ValidResult;
                }

                if (!(value is string))
                {
                    return new ValidationResult(false, "Введены неверные данные");
                }

                if (!System.IO.Directory.Exists((string)value))
                {
                    return new ValidationResult(false, "Путь не найден");
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
