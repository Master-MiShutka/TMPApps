namespace TMPApplication.WpfDialogs
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using TMPApplication.WpfDialogs.Contracts;

    internal class MessageDialog : DialogBase, IMessageDialog
    {
        public MessageDialog(
            IDialogHost dialogHost,
            DialogMode dialogMode,
            string message,
            Dispatcher dispatcher,
            System.Windows.MessageBoxImage image = MessageBoxImage.None)
            : base(dialogHost, dialogMode, dispatcher, image)
        {
            this.HorizontalDialogAlignment = HorizontalAlignment.Center;
            this.VerticalDialogAlignment = VerticalAlignment.Center;

            this.InvokeUICall(() =>
                {
                    this.messageTextBlock = new TextBlock
                    {
                        Text = message,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
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