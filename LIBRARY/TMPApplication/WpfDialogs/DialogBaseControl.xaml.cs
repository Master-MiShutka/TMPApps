namespace TMPApplication.WpfDialogs
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for DialogBaseControl.xaml
    /// </summary>
    partial class DialogBaseControl : INotifyPropertyChanged
    {
        public DialogBaseControl(DialogBase dialog, Control background = null)
        {
            this.Caption = dialog.Caption;

            this.dialog = dialog;

            this.InitializeComponent();

            this.CreateButtons();

            if (background != null)
            {
                this.BackgroundImageHolder.Content = background;
            }
        }

        private readonly DialogBase dialog;

        public System.Windows.MessageBoxImage Image => this.dialog != null ? this.dialog.Image : MessageBoxImage.None;

        public Visibility ImageVisibility => this.Image == MessageBoxImage.None
                    ? Visibility.Collapsed
                    : Visibility.Visible;

        public string Caption { get; private set; }

        public Visibility CaptionVisibility => string.IsNullOrWhiteSpace(this.Caption)
                    ? Visibility.Collapsed
                    : Visibility.Visible;

        public Visibility ButtonsVisibility
        {
            get
            {
                if (this.dialog == null)
                {
                    return Visibility.Collapsed;
                }

                if (this.dialog.Mode == DialogMode.None)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        private VerticalAlignment verticalDialogAlignment = VerticalAlignment.Center;

        public VerticalAlignment VerticalDialogAlignment
        {
            get => this.verticalDialogAlignment;
            set
            {
                this.verticalDialogAlignment = value;
                this.OnPropertyChanged(nameof(this.VerticalDialogAlignment));
            }
        }

        private HorizontalAlignment horizontalDialogAlignment = HorizontalAlignment.Center;

        public HorizontalAlignment HorizontalDialogAlignment
        {
            get => this.horizontalDialogAlignment;
            set
            {
                this.horizontalDialogAlignment = value;
                this.OnPropertyChanged(nameof(this.HorizontalDialogAlignment));
            }
        }

        public void SetCustomContent(object content)
        {
            if (content is string str)
            {
                this.CustomContent.Visibility = Visibility.Collapsed;
                this.ScrollContent.Visibility = Visibility.Visible;
                this.StringContent.Content = str;
            }
            else
            {
                this.CustomContent.Visibility = Visibility.Visible;
                this.ScrollContent.Visibility = Visibility.Collapsed;
                this.CustomContent.Content = content;
            }
        }

        private void CreateButtons()
        {
            switch (this.dialog.Mode)
            {
                case DialogMode.None:
                    break;
                case DialogMode.Ok:
                    this.AddOkButton();
                    break;
                case DialogMode.Cancel:
                    this.AddCancelButton();
                    break;
                case DialogMode.OkCancel:
                    this.AddOkButton();
                    this.AddCancelButton();
                    break;
                case DialogMode.YesNo:
                    this.AddYesButton();
                    this.AddNoButton();
                    break;
                case DialogMode.YesNoCancel:
                    this.AddYesButton();
                    this.AddNoButton();
                    this.AddCancelButton();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.OnPropertyChanged(nameof(this.ButtonsVisibility));
        }

        public void AddNoButton()
        {
            this.AddButton(this.dialog.NoText, this.GetCallback(this.dialog.No, DialogResultState.No), false, true, "CanNo");
        }

        public void AddYesButton()
        {
            this.AddButton(this.dialog.YesText, this.GetCallback(this.dialog.Yes, DialogResultState.Yes), true, false, "CanYes");
        }

        public void AddCancelButton()
        {
            this.AddButton(this.dialog.CancelText, this.GetCallback(this.dialog.Cancel, DialogResultState.Cancel), false, true, "CanCancel");
        }

        public void AddOkButton()
        {
            this.AddButton(this.dialog.OkText, this.GetCallback(this.dialog.Ok, DialogResultState.Ok), true, true, "CanOk");
        }

        private void AddButton(
            string buttonText,
            Action callback,
            bool isDefault,
            bool isCancel,
            string bindingPath)
        {
            var btn = new Button
            {
                Content = buttonText,
                MinWidth = 80,
                MaxWidth = 450,
                IsDefault = isDefault,
                IsCancel = isCancel,
                Margin = new Thickness(5),
            };

            btn.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);

            var enabledBinding = new Binding(bindingPath) { Source = this.dialog };
            btn.SetBinding(IsEnabledProperty, enabledBinding);

            btn.Click += (s, e) => callback();

            this.ButtonsGrid.Columns++;
            this.ButtonsGrid.Children.Add(btn);
        }

        internal void RemoveButtons()
        {
            this.ButtonsGrid.Children.Clear();
        }

        private Action GetCallback(
            Action dialogCallback,
            DialogResultState result)
        {
            this.dialog.Result = result;
            Action callback = () =>
            {
                dialogCallback?.Invoke();
                if (this.dialog.CloseBehavior == DialogCloseBehavior.AutoCloseOnButtonClick)
                {
                    this.dialog.Close();
                }
            };

            return callback;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
