﻿namespace TMP.PrintEngine.Controls.ProgressDialog
{
    using System.Windows;
    using System.Windows.Input;
    using TMP.PrintEngine.Controls.WaitScreen;

    public interface IProgressDialogViewModel : IWaitScreenViewModel
    {
        string DialogTitle { get; set; }

        string CancelButtonCaption { get; set; }

        double MaxProgressValue { get; set; }

        double CurrentProgressValue { get; set; }

        Visibility CancelButtonVisibility { get; set; }

        ICommand CancelCommand { get; set; }

        void Initialize(ICommand cancelCommand, int maxProgressValue);
    }
}
