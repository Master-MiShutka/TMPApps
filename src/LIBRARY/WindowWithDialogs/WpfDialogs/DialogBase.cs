namespace WindowWithDialogs.WpfDialogs
{
    using System;
    using System.ComponentModel;
    using System.Windows.Threading;
    using UIInfrastructure.Dialogs.Contracts;

    public abstract class DialogBase : IDialog, INotifyPropertyChanged
    {
        private readonly IDialogHost dialogHost;
        private readonly Dispatcher dispatcher;
        private object _content;

        private bool canCancel;

        private bool canYes;
        private bool canNo;
        private UIInfrastructure.VerticalAlignment verticalDialogAlignment;
        private UIInfrastructure.HorizontalAlignment _horizontalDialogAlignment;

        protected DialogBase(
            IDialogHost dialogHost,
            UIInfrastructure.DialogMode dialogMode,
            Dispatcher dispatcher,
            UIInfrastructure.MessageBoxImage image = UIInfrastructure.MessageBoxImage.None)
        {
            this.dialogHost = dialogHost;
            this.dispatcher = dispatcher;
            this.Mode = dialogMode;
            this.Image = image;
            this.CloseBehavior = UIInfrastructure.DialogCloseBehavior.AutoCloseOnButtonClick;

            this.OkText = Strings.OK_Button_Label;
            this.CancelText = Strings.Cancel_Button_Label;
            this.YesText = Strings.Yes_Button_Label;
            this.NoText = Strings.No_Button_Label;

            switch (dialogMode)
            {
                case UIInfrastructure.DialogMode.None:
                    break;
                case UIInfrastructure.DialogMode.Ok:
                    this.CanOk = true;
                    break;
                case UIInfrastructure.DialogMode.Cancel:
                    this.CanCancel = true;
                    break;
                case UIInfrastructure.DialogMode.OkCancel:
                    this.CanOk = true;
                    this.CanCancel = true;
                    break;
                case UIInfrastructure.DialogMode.YesNo:
                    this.CanYes = true;
                    this.CanNo = true;
                    break;
                case UIInfrastructure.DialogMode.YesNoCancel:
                    this.CanYes = true;
                    this.CanNo = true;
                    this.CanCancel = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogMode));
            }
        }

        protected DialogBaseControl DialogBaseControl { get; private set; }

        protected void SetContent(object content)
        {
            this._content = content;
        }

        protected void InvokeUICall(Action del)
        {
            this.dispatcher.Invoke(del, DispatcherPriority.DataBind);
        }

        #region Implementation of IDialog

        public UIInfrastructure.DialogMode Mode { get; private set; }

        public UIInfrastructure.DialogResultState Result { get; set; }

        public UIInfrastructure.DialogCloseBehavior CloseBehavior { get; set; }

        public Action? Ok { get; set; }

        public Action? Cancel { get; set; }

        public Action? Yes { get; set; }

        public Action? No { get; set; }

        private bool canOk;

        public bool CanOk
        {
            get => this.canOk;
            set
            {
                this.canOk = value;
                this.OnPropertyChanged(nameof(this.CanOk));
            }
        }

        public bool CanCancel
        {
            get => this.canCancel;
            set
            {
                this.canCancel = value;
                this.OnPropertyChanged(nameof(this.CanCancel));
            }
        }

        public bool CanYes
        {
            get => this.canYes;
            set
            {
                this.canYes = value;
                this.OnPropertyChanged(nameof(this.CanYes));
            }
        }

        public bool CanNo
        {
            get => this.canNo;
            set
            {
                this.canNo = value;
                this.OnPropertyChanged(nameof(this.CanNo));
            }
        }

        public string OkText { get; set; }

        public string CancelText { get; set; }

        public string YesText { get; set; }

        public string NoText { get; set; }

        public string Caption { get; set; }

        public UIInfrastructure.VerticalAlignment VerticalDialogAlignment
        {
            set
            {
                if (this.DialogBaseControl == null)
                {
                    this.verticalDialogAlignment = value;
                }
                else
                {
                    this.DialogBaseControl.VerticalDialogAlignment = value;
                }
            }
        }

        public UIInfrastructure.HorizontalAlignment HorizontalDialogAlignment
        {
            set
            {
                if (this.DialogBaseControl == null)
                {
                    this._horizontalDialogAlignment = value;
                }
                else
                {
                    this.DialogBaseControl.HorizontalDialogAlignment = value;
                }
            }
        }

        public object Background { get; set; }

        public UIInfrastructure.MessageBoxImage Image { get; set; } = UIInfrastructure.MessageBoxImage.None;

        public void Show()
        {
            if (this.DialogBaseControl != null)
            {
                throw new Exception("The dialog can only be shown once.");
            }

            this.InvokeUICall(() =>
                {
                    this.DialogBaseControl = new DialogBaseControl(this, this.Background);
                    this.DialogBaseControl.SetCustomContent(this._content);
                    this.DialogBaseControl.VerticalDialogAlignment = this.verticalDialogAlignment;

                    this.DialogBaseControl.HorizontalDialogAlignment = this._horizontalDialogAlignment;

                    this.dialogHost.ShowDialog(this.DialogBaseControl);
                });
        }

        public void Close()
        {
            // Dialog wird angezeigt?
            if (this.DialogBaseControl == null)
            {
                return;
            }

            // Callbacks abhängen
            this.Ok = null;
            this.Cancel = null;
            this.Yes = null;
            this.No = null;

            this.InvokeUICall(
                () =>
                {
                    this.dialogHost.HideDialog(this.DialogBaseControl);
                    this.DialogBaseControl.SetCustomContent(null);
                });
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}