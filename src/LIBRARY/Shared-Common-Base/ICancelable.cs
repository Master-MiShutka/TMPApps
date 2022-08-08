namespace TMP.Shared.Common
{
    using System.Windows.Input;

    public interface ICancelable
    {
        ICommand CancelCommand { get; set; }

        bool IsCanceled { get; set; }

        bool CanCanceled { get; }
    }
}
