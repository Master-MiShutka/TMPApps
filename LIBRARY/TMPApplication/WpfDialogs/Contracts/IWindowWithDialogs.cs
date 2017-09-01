using System;

namespace TMPApplication.WpfDialogs.Contracts
{
    public interface IWindowWithDialogs
    {
        System.Windows.Controls.Grid DialogsContainer { get; }

        IDialogManager DialogManager { get; }

        IDialog DialogError(string message, string caption = null, Action ok = null);
        IDialog DialogError(Exception e);
        IDialog DialogError(Exception e, string format);
        IDialog DialogWarning(string message, string caption = null, Action ok = null);
        IDialog DialogInfo(string message);
        IDialog DialogInfo(string message, string caption = null, Action ok = null);
        IDialog DialogInfo(string message, string caption = null, System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.None,
            DialogMode mode = DialogMode.Ok);

        IDialog DialogQuestion(string message, string caption = null, DialogMode mode = DialogMode.YesNo);

        IDialog DialogWaitingScreen(string message, bool indeterminate = true, DialogMode mode = DialogMode.None);
        IDialog DialogCustom(System.Windows.Controls.Control content, DialogMode mode = DialogMode.None);
        IDialog DialogProgress(string message, Action action, DialogMode mode = DialogMode.None, bool indeterminate = true);

        void ShowDialogError(string message, string caption = null, Action ok = null);
        void ShowDialogError(Exception e);
        void ShowDialogError(Exception e, string format);
        void ShowDialogWarning(string message, string caption = null, Action ok = null);
        void ShowDialogInfo(string message);
        void ShowDialogInfo(string message, string caption = null, Action ok = null);
        void ShowDialogInfo(string message, string caption = null, System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.None,
            DialogMode mode = DialogMode.Ok);

        void ShowDialogQuestion(string message, string caption = null, DialogMode mode = DialogMode.YesNo);

        void ShowDialogWaitingScreen(string message, bool indeterminate = true, DialogMode mode = DialogMode.None);
        void ShowDialogCustom(System.Windows.Controls.Control content, DialogMode mode = DialogMode.None);
        void ShowDialogProgress(string message, Action action, DialogMode mode = DialogMode.None, bool indeterminate = true);
    }
}