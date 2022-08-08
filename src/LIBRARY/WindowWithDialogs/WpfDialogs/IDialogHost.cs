namespace WindowWithDialogs.WpfDialogs
{
    using System.Windows;

    public interface IDialogHost
    {
        void ShowDialog(DialogBaseControl dialog);

        void HideDialog(DialogBaseControl dialog);

        FrameworkElement GetCurrentContent();
    }
}