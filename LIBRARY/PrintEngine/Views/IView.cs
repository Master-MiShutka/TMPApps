using TMP.PrintEngine.ViewModels;

namespace TMP.PrintEngine.Views
{
    public interface IView
    {
        IViewModel ViewModel { set; }
    }
}
