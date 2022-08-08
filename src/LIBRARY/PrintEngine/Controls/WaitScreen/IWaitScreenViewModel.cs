namespace TMP.PrintEngine.Controls.WaitScreen
{
    using TMP.PrintEngine.ViewModels;

    public interface IWaitScreenViewModel : IViewModel
    {
        bool Hide();

        bool Show();

        bool Show(string message);

        bool Show(string message, bool disableParent);

        string Message { get; set; }

        void HideNow();
    }
}
