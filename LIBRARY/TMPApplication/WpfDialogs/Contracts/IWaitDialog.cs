using System;

namespace TMPApplication.WpfDialogs.Contracts
{
	public interface IWaitDialog : IMessageDialog
	{
		Action WorkerReady { get; set; }

		bool CloseWhenWorkerFinished { get; set; }
		string ReadyMessage { get; set; }

		void Show(Action workerMethod);
		void InvokeUICall(Action uiWorker);
	}
}