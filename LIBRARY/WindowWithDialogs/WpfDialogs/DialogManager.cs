namespace WindowWithDialogs
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Contracts;

    public class DialogManager : IDialogManager
    {
        public DialogManager(
            Grid parent,
            Dispatcher dispatcher,
            UIElement uIElement)
        {
            this.dispatcher = dispatcher;
            this.dialogHost = new DialogLayeringHelper(parent, uIElement);
        }

        private readonly Dispatcher dispatcher;
        private readonly IDialogHost dialogHost;

        #region Implementation of IDialogManager

        public IMessageDialog CreateMessageDialog(string message, DialogMode dialogMode, System.Windows.MessageBoxImage image = MessageBoxImage.None)
        {
            IMessageDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = new MessageDialog(this.dialogHost, dialogMode, message, this.dispatcher, image);
            });
            return dialog;
        }

        public IMessageDialog CreateMessageDialog(string message, string caption, DialogMode dialogMode, System.Windows.MessageBoxImage image = MessageBoxImage.None)
        {
            IMessageDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = new MessageDialog(this.dialogHost, dialogMode, message, this.dispatcher, image)
                {
                    Caption = caption,
                };
            });
            return dialog;
        }

        public IProgressDialog CreateProgressDialog(DialogMode dialogMode, bool isIndeterminate = false)
        {
            IProgressDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = WaitProgressDialog.CreateProgressDialog(this.dialogHost, dialogMode, this.dispatcher, isIndeterminate);
                dialog.CloseWhenWorkerFinished = true;
            });
            return dialog;
        }

        public IProgressDialog CreateProgressDialog(string message, DialogMode dialogMode, bool isIndeterminate = false)
        {
            IProgressDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = WaitProgressDialog.CreateProgressDialog(this.dialogHost, dialogMode, this.dispatcher, isIndeterminate);
                dialog.CloseWhenWorkerFinished = true;
                dialog.Message = message;
            });
            return dialog;
        }

        public IProgressDialog CreateProgressDialog(string message, string readyMessage, DialogMode dialogMode, bool isIndeterminate = false)
        {
            IProgressDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = WaitProgressDialog.CreateProgressDialog(this.dialogHost, dialogMode, this.dispatcher, isIndeterminate);
                dialog.CloseWhenWorkerFinished = false;
                dialog.ReadyMessage = readyMessage;
                dialog.Message = message;
            });
            return dialog;
        }

        public IWaitDialog CreateWaitDialog(DialogMode dialogMode, bool isIndeterminate = false)
        {
            IWaitDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = WaitProgressDialog.CreateWaitDialog(this.dialogHost, dialogMode, this.dispatcher, isIndeterminate);
                dialog.CloseWhenWorkerFinished = true;
            });
            return dialog;
        }

        public IWaitDialog CreateWaitDialog(string message, DialogMode dialogMode, bool isIndeterminate = false)
        {
            IWaitDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = WaitProgressDialog.CreateWaitDialog(this.dialogHost, dialogMode, this.dispatcher, isIndeterminate);
                dialog.CloseWhenWorkerFinished = true;
                dialog.Message = message;
            });
            return dialog;
        }

        public IWaitDialog CreateWaitDialog(string message, string readyMessage, DialogMode dialogMode, bool isIndeterminate = false)
        {
            IWaitDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = WaitProgressDialog.CreateWaitDialog(this.dialogHost, dialogMode, this.dispatcher, isIndeterminate);
                dialog.CloseWhenWorkerFinished = false;
                dialog.Message = message;
                dialog.ReadyMessage = readyMessage;
            });
            return dialog;
        }

        public ICustomContentDialog CreateCustomContentDialog(object content, DialogMode dialogMode)
        {
            ICustomContentDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = new CustomContentDialog(this.dialogHost, dialogMode, content, this.dispatcher);
            });
            return dialog;
        }

        public ICustomContentDialog CreateCustomContentDialog(object content, string caption, DialogMode dialogMode)
        {
            ICustomContentDialog dialog = null;
            this.InvokeInUIThread(() =>
            {
                dialog = new CustomContentDialog(this.dialogHost, dialogMode, content, this.dispatcher)
                {
                    Caption = caption,
                };
            });
            return dialog;
        }

        #endregion

        private void InvokeInUIThread(Action del)
        {
            this.dispatcher.Invoke(del);
        }
    }
}