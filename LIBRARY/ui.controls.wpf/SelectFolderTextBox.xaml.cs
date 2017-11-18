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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.UI.Controls.WPF
{
    /// <summary>
    /// Interaction logic for SelectFolderTextBox.xaml
    /// </summary>
    [TemplatePart(Name = ElementLabel, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementText, Type = typeof(TextBox))]
    [TemplatePart(Name = ElementButton, Type = typeof(Button))]
    public partial class SelectFolderTextBox : UserControl
    {
        private const string ElementLabel = "PART_Label";
        private const string ElementText = "PART_Text";
        private const string ElementButton = "PART_Select_Folder_Button";

        private TextBlock _label;
        private TextBox _text;
        private Button _button;

        public SelectFolderTextBox()
        {
            InitializeComponent();
            Root.DataContext = this;

            SelectFolder = new DelegateCommand(() =>
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                dialog.Description = "Выберите папку";
                var result = dialog.ShowDialog(Application.Current.MainWindow);
                if (result != null && result.HasValue)
                {
                    SelectedPath = dialog.SelectedPath;
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
                _button.Command = SelectFolder;


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
                typeof(SelectFolderTextBox),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty
            .Register("SelectedPath",
                    typeof(string),
                    typeof(SelectFolderTextBox),
                    new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedPathChanged));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        private static void SelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null) return;
            SelectFolderTextBox sftb = d as SelectFolderTextBox;
            if (sftb == null || sftb._text == null) return;
            sftb._text.Text = e.NewValue.ToString();
        }

        public ICommand SelectFolder { get; private set; }
    }

    public class DirectoryExistsRule : ValidationRule
    {
        public static DirectoryExistsRule Instance = new DirectoryExistsRule();

        public override ValidationResult Validate(object value,
               System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                if (!(value is string))
                    return new ValidationResult(false, "Введены неверные данные");

                if (!System.IO.Directory.Exists((string)value))
                    return new ValidationResult(false, "Путь не найден");
            }
            catch (Exception)
            {
                return new ValidationResult(false, "Неверный путь");
            }
            return new ValidationResult(true, null);
        }
    }
}
