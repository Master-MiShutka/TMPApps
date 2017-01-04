using TMP.Shared;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    public interface IEzSbytServiceFunctionViewModel : IWaitableObject
    {
        ResultViewerViewModel Result { get; set; }
    }
}