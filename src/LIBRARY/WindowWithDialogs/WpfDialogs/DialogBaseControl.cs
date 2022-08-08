namespace WindowWithDialogs.WpfDialogs
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

    [TemplatePart(Name = "Part_BackgroundImageHolder", Type = typeof(ContentControl))]
    [TemplatePart(Name = "Part_CustomContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "Part_StringContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "Part_ScrollContent", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "Part_ButtonsGrid", Type = typeof(UniformGrid))]
    public class DialogBaseControl : ContentControl, INotifyPropertyChanged
    {
        private ContentControl backgroundImageHolder, customContent, stringContent;
        private ScrollViewer scrollViewer;
        private UniformGrid buttonsGrid;

        static DialogBaseControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogBaseControl), new FrameworkPropertyMetadata(typeof(DialogBaseControl)));
        }

        public DialogBaseControl(DialogBase dialog, object background = null)
        {
            this.Caption = dialog.Caption;

            this.dialog = dialog;

            this.CreateButtons();

            if (background != null && this.backgroundImageHolder != null)
            {
                this.backgroundImageHolder.Content = background;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.backgroundImageHolder = this.EnforceInstance<ContentControl>("Part_BackgroundImageHolder");
            this.stringContent = this.EnforceInstance<ContentControl>("Part_StringContent");
            this.customContent = this.EnforceInstance<ContentControl>("Part_CustomContent");

            this.scrollViewer = this.EnforceInstance<ScrollViewer>("Part_ScrollContent");
            this.buttonsGrid = this.EnforceInstance<UniformGrid>("Part_ButtonsGrid");
        }

        // Get element from name. If it exist then element instance return, if not, new will be created
        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            T element = this.GetTemplateChild(partName) as T ?? new T();

            return element ?? throw new NullReferenceException(nameof(partName));
        }

        private readonly DialogBase dialog;

        public UIInfrastructure.MessageBoxImage Image => this.dialog != null ? this.dialog.Image : UIInfrastructure.MessageBoxImage.None;

        public Visibility ImageVisibility => this.Image == UIInfrastructure.MessageBoxImage.None
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

                if (this.dialog.Mode == UIInfrastructure.DialogMode.None)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        private UIInfrastructure.VerticalAlignment verticalDialogAlignment = UIInfrastructure.VerticalAlignment.Center;

        public UIInfrastructure.VerticalAlignment VerticalDialogAlignment
        {
            get => this.verticalDialogAlignment;
            set
            {
                this.verticalDialogAlignment = value;
                this.OnPropertyChanged(nameof(this.VerticalDialogAlignment));
            }
        }

        private UIInfrastructure.HorizontalAlignment horizontalDialogAlignment = UIInfrastructure.HorizontalAlignment.Center;

        public UIInfrastructure.HorizontalAlignment HorizontalDialogAlignment
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
                this.customContent.Visibility = Visibility.Collapsed;
                this.scrollViewer.Visibility = Visibility.Visible;
                this.stringContent.Content = str;
            }
            else
            {
                this.customContent.Visibility = Visibility.Visible;
                this.scrollViewer.Visibility = Visibility.Collapsed;
                this.customContent.Content = content;
            }
        }

        private void CreateButtons()
        {
            switch (this.dialog.Mode)
            {
                case UIInfrastructure.DialogMode.None:
                    break;
                case UIInfrastructure.DialogMode.Ok:
                    this.AddOkButton();
                    break;
                case UIInfrastructure.DialogMode.Cancel:
                    this.AddCancelButton();
                    break;
                case UIInfrastructure.DialogMode.OkCancel:
                    this.AddOkButton();
                    this.AddCancelButton();
                    break;
                case UIInfrastructure.DialogMode.YesNo:
                    this.AddYesButton();
                    this.AddNoButton();
                    break;
                case UIInfrastructure.DialogMode.YesNoCancel:
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
            this.AddButton(this.dialog.NoText, this.GetCallback(this.dialog.No, UIInfrastructure.DialogResultState.No), false, true, "CanNo");
        }

        public void AddYesButton()
        {
            this.AddButton(this.dialog.YesText, this.GetCallback(this.dialog.Yes, UIInfrastructure.DialogResultState.Yes), true, false, "CanYes");
        }

        public void AddCancelButton()
        {
            this.AddButton(this.dialog.CancelText, this.GetCallback(this.dialog.Cancel, UIInfrastructure.DialogResultState.Cancel), false, true, "CanCancel");
        }

        public void AddOkButton()
        {
            this.AddButton(this.dialog.OkText, this.GetCallback(this.dialog.Ok, UIInfrastructure.DialogResultState.Ok), true, true, "CanOk");
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

            this.buttonsGrid.Columns++;
            this.buttonsGrid.Children.Add(btn);
        }

        internal void RemoveButtons()
        {
            this.buttonsGrid.Children.Clear();
        }

        private Action GetCallback(
            Action? dialogCallback,
            UIInfrastructure.DialogResultState result)
        {
            this.dialog.Result = result;
            Action callback = () =>
            {
                dialogCallback?.Invoke();
                if (this.dialog.CloseBehavior == UIInfrastructure.DialogCloseBehavior.AutoCloseOnButtonClick)
                {
                    this.dialog.Close();
                }
            };

            return callback;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
