namespace TMP.PrintEngine.ViewModels
{
    public interface IPrintControlViewModel : IViewModel
    {
        bool CanScale { get; set; }
        void ShowPrintPreview();
    }
}