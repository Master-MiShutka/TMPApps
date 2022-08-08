namespace WindowWithDialogs.WpfDialogs
{
    using System.Windows.Threading;
    using UIInfrastructure.Dialogs.Contracts;

    internal class CustomContentDialog : DialogBase, ICustomContentDialog
    {
        public CustomContentDialog(
            IDialogHost dialogHost,
            UIInfrastructure.DialogMode dialogMode,
            object content,
            Dispatcher dispatcher)
            : base(dialogHost, dialogMode, dispatcher)
        {
            if (content is System.Windows.Controls.Control control)
            {
                this.HorizontalDialogAlignment = Utils.ToUIInfrastructure(control.HorizontalAlignment);
                this.VerticalDialogAlignment = Utils.ToUIInfrastructure(control.VerticalAlignment);
            }

            this.SetContent(content);
        }
    }
}