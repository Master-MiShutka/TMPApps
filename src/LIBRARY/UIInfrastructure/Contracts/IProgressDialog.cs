namespace UIInfrastructure.Contracts
{
    using System;

    public interface IProgressDialog : IWaitDialog
    {
        double Progress { get; set; }

        bool IsIndeterminate { get; set; }

        void Show(Action<IProgressDialog> workerMethod);
    }
}