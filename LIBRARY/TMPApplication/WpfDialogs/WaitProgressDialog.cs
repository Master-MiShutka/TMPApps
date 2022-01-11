namespace TMPApplication.WpfDialogs
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using TMPApplication.WpfDialogs.Contracts;

    internal class WaitProgressDialog : DialogBase, IProgressDialog
    {
        #region Factory

        public static IWaitDialog CreateWaitDialog(
            IDialogHost dialogHost,
            DialogMode dialogMode,
            Dispatcher dispatcher,
            bool isIndeterminate)
        {
            IWaitDialog dialog = null;
            dispatcher.Invoke(
                new Action(() => dialog = new WaitProgressDialog(
                    dialogHost, dialogMode, isIndeterminate, dispatcher)),
                DispatcherPriority.DataBind);
            return dialog;
        }

        public static IProgressDialog CreateProgressDialog(
            IDialogHost dialogHost,
            DialogMode dialogMode,
            Dispatcher dispatcher,
            bool isIndeterminate)
        {
            IProgressDialog dialog = null;
            dispatcher.Invoke(
                new Action(() => dialog = new WaitProgressDialog(
                    dialogHost, dialogMode, isIndeterminate, dispatcher)),
                DispatcherPriority.DataBind);
            return dialog;
        }

        #endregion

        private WaitProgressDialog(
            IDialogHost dialogHost,
            DialogMode dialogMode,
            bool isIndeterminate,
            Dispatcher dispatcher,
            System.Windows.MessageBoxImage image = MessageBoxImage.None)
            : base(dialogHost, dialogMode, dispatcher, image)
        {
            this.HorizontalDialogAlignment = HorizontalAlignment.Center;
            this.VerticalDialogAlignment = VerticalAlignment.Center;

            this.waitProgressDialogControl = new WaitProgressDialogControl(isIndeterminate);
            this.SetContent(this.waitProgressDialogControl);
        }

        private readonly WaitProgressDialogControl waitProgressDialogControl;
        private bool isReady;

        #region Implementation of IMessageDialog

        public string Message
        {
            get
            {
                var text = string.Empty;
                this.InvokeUICall(
                    () => text = this.waitProgressDialogControl.DisplayText);
                return text;
            }
            set => this.InvokeUICall(
                    () => this.waitProgressDialogControl.DisplayText = value);
        }

        #endregion

        #region Implementation of IWaitDialog

        public Action WorkerReady { get; set; }

        public bool CloseWhenWorkerFinished { get; set; }

        private string _readyMessage;

        public string ReadyMessage
        {
            get => this._readyMessage;
            set
            {
                this._readyMessage = value;
                if (this.isReady)
                {
                    this.InvokeUICall(
                        () => this.waitProgressDialogControl.DisplayText = value);
                }
            }
        }

        private readonly ManualResetEvent _beginWork =
            new ManualResetEvent(false);

        public void Show(Action workerMethod)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    this._beginWork.WaitOne(-1);

                    workerMethod();

                    this.InvokeUICall(() =>
                    {
                        this.isReady = true;

                        if (this.WorkerReady != null)
                        {
                            this.WorkerReady();
                        }

                        if (this.CloseWhenWorkerFinished)
                        {
                            this.Close();
                            return;
                        }

                        this.waitProgressDialogControl.DisplayText = this.ReadyMessage;
                        this.waitProgressDialogControl.Finish();

                        this.DialogBaseControl.RemoveButtons();
                        this.DialogBaseControl.AddOkButton();
                    });
                }
                catch (Exception ex)
                {
                    this.InvokeUICall(() =>
                    {
                        this.Close();
                        throw ex;
                    });
                }
            });

            this.Show();

            this._beginWork.Set();
        }

        public void Show(Action<IProgressDialog> workerMethod)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    this._beginWork.WaitOne(-1);

                    workerMethod(this);

                    this.InvokeUICall(() =>
                    {
                        this.isReady = true;

                        if (this.WorkerReady != null)
                        {
                            this.WorkerReady();
                        }

                        if (this.CloseWhenWorkerFinished)
                        {
                            this.Close();
                            return;
                        }

                        this.waitProgressDialogControl.DisplayText = this.ReadyMessage;
                        this.waitProgressDialogControl.Finish();

                        this.DialogBaseControl.RemoveButtons();
                        this.DialogBaseControl.AddOkButton();
                    });
                }
                catch (Exception ex)
                {
                    this.InvokeUICall(() =>
                    {
                        this.Close();
                        throw ex;
                    });
                }
            });

            this.Show();

            this._beginWork.Set();
        }

        public new void InvokeUICall(Action uiWorker)
        {
            base.InvokeUICall(uiWorker);
        }

        #endregion

        #region Implementation of IProgressDialog

        public double Progress
        {
            get
            {
                double progress = 0;
                this.InvokeUICall(
                    () => progress = this.waitProgressDialogControl.Progress);
                return progress;
            }
            set => this.InvokeUICall(
                    () => this.waitProgressDialogControl.Progress = value);
        }

        public bool IsIndeterminate
        {
            get
            {
                bool isIndeterminate = false;
                this.InvokeUICall(
                    () => isIndeterminate = this.waitProgressDialogControl.IsIndeterminate);
                return isIndeterminate;
            }
            set => this.InvokeUICall(
                    () => this.waitProgressDialogControl.IsIndeterminate = value);
        }

        #endregion
    }
}