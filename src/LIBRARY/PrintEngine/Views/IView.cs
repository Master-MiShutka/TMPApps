namespace TMP.PrintEngine.Views
{
    using TMP.PrintEngine.ViewModels;

    public interface IView
    {
        IViewModel ViewModel { set; }
    }
}
