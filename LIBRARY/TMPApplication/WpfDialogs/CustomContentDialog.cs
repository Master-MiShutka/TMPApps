namespace TMPApplication.WpfDialogs
{
    using System.Windows.Threading;
    using TMPApplication.WpfDialogs.Contracts;

    internal class CustomContentDialog : DialogBase, ICustomContentDialog
    {
        public CustomContentDialog(
            IDialogHost dialogHost,
            DialogMode dialogMode,
            object content,
            Dispatcher dispatcher)
            : base(dialogHost, dialogMode, dispatcher)
        {
            if (content is System.Windows.Controls.Control control)
            {
                this.HorizontalDialogAlignment = control.HorizontalAlignment;
                this.VerticalDialogAlignment = control.VerticalAlignment;
            }

            this.SetContent(content);
        }
    }
}