using System;
namespace TMPApplication.WpfDialogs.Contracts
{
	public interface IProgressDialog : IWaitDialog
	{
		int Progress { get; set; }
        void Show(Action<IProgressDialog> workerMethod);
    }
}