namespace WindowWithDialogs.WpfDialogs
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using UIInfrastructure.Dialogs.Contracts;

    internal class MessageDialog : DialogBase, IMessageDialog
    {
        public MessageDialog(
            IDialogHost dialogHost,
            UIInfrastructure.DialogMode dialogMode,
            string message,
            Dispatcher dispatcher,
            UIInfrastructure.MessageBoxImage image = UIInfrastructure.MessageBoxImage.None)
            : base(dialogHost, dialogMode, dispatcher, image)
        {
            this.HorizontalDialogAlignment = UIInfrastructure.HorizontalAlignment.Center;
            this.VerticalDialogAlignment = UIInfrastructure.VerticalAlignment.Center;

            this.InvokeUICall(() =>
                {
                    this.messageTextBlock = new TextBlock
                    {
                        Text = message,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                    };
                    this.SetContent(this.messageTextBlock);
                });
        }

        private TextBlock messageTextBlock;

        #region Implementation of IMessageDialog

        public string Message
        {
            get
            {
                var text = string.Empty;
                this.InvokeUICall(
                    () => text = this.messageTextBlock.Text);
                return text;
            }
            set => this.InvokeUICall(
                    () => this.messageTextBlock.Text = value);
        }

        #endregion
    }
}