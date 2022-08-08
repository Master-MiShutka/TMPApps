namespace TMPApplication.WpfDialogs
{
    using System.Windows;

    internal interface IDialogHost
    {
        void ShowDialog(DialogBaseControl dialog);

        void HideDialog(DialogBaseControl dialog);

        FrameworkElement GetCurrentContent();
    }
}