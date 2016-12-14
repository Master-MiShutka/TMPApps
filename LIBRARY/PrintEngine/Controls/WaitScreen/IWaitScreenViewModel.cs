using TMP.PrintEngine.ViewModels;

namespace TMP.PrintEngine.Controls.WaitScreen
{
    public interface IWaitScreenViewModel:IViewModel
    {
        bool Hide();
        bool Show();
        bool Show(string message);
        bool Show(string message, bool disableParent);
        string Message { get; set; }
        void HideNow();
    }
}
