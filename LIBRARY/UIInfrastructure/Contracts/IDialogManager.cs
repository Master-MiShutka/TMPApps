namespace UIInfrastructure.WindowWithDialogs.Contracts
{
    public interface IDialogManager
    {
        IMessageDialog CreateMessageDialog(string message, DialogMode dialogMode, System.Windows.MessageBoxImage image);

        IMessageDialog CreateMessageDialog(string message, string caption, DialogMode dialogMode, System.Windows.MessageBoxImage image);

        ICustomContentDialog CreateCustomContentDialog(object content, DialogMode dialogMode);

        ICustomContentDialog CreateCustomContentDialog(object content, string caption, DialogMode dialogMode);

        IProgressDialog CreateProgressDialog(DialogMode dialogMode, bool isIndeterminate = false);

        IProgressDialog CreateProgressDialog(string message, DialogMode dialogMode, bool isIndeterminate = false);

        IProgressDialog CreateProgressDialog(string message, string readyMessage, DialogMode dialogMode, bool isIndeterminate = false);

        IWaitDialog CreateWaitDialog(DialogMode dialogMode, bool isIndeterminate = false);

        IWaitDialog CreateWaitDialog(string message, DialogMode dialogMode, bool isIndeterminate = false);

        IWaitDialog CreateWaitDialog(string message, string readyMessage, DialogMode dialogMode, bool isIndeterminate = false);
    }
}