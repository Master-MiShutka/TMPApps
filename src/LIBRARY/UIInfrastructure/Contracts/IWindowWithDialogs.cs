namespace UIInfrastructure.Contracts
{
    using System;

    public interface IWindowWithDialogs
    {
        object DialogsContainer { get; }

        IDialogManager DialogManager { get; }

        IDialog LastDialog { get; }

        IDialog DialogError(string message, string? caption = null, Action? ok = null);

        IDialog DialogError(Exception e);

        IDialog DialogError(Exception e, string format);

        IDialog DialogWarning(string message, string? caption = default, Action? ok = null);

        IDialog DialogInfo(string message, string? caption = null);

        IDialog DialogInfo(string message, string? caption = null, Action? ok = null);

        IDialog DialogInfo(string message, string? caption = null, MessageBoxImage image = MessageBoxImage.None,
            DialogMode mode = DialogMode.Ok);

        IDialog DialogQuestion(string message, string? caption = null, DialogMode mode = DialogMode.YesNo);

        IWaitDialog DialogWaitingScreen(string message, string? caption = null, bool indeterminate = true, DialogMode mode = DialogMode.None);

        ICustomContentDialog DialogCustom(object content, string? caption = null, DialogMode mode = DialogMode.None);

        IProgressDialog DialogProgress(string message, string? caption = null, DialogMode mode = DialogMode.None, bool indeterminate = true);

        void ShowDialogError(string message, string? caption = null, Action? ok = null);

        void ShowDialogError(Exception e);

        void ShowDialogError(Exception e, string format);

        void ShowDialogWarning(string message, string? caption = null, Action? ok = null);

        void ShowDialogInfo(string message, string? caption = null);

        void ShowDialogInfo(string message, string? caption = null, Action? ok = null);

        void ShowDialogInfo(string message, string? caption = null, MessageBoxImage image = MessageBoxImage.None,
            DialogMode mode = DialogMode.Ok);

        void ShowDialogQuestion(string message, string? caption = null, DialogMode mode = DialogMode.YesNo);

        void ShowDialogWaitingScreen(string message, string? caption = null, bool indeterminate = true, DialogMode mode = DialogMode.None);

        void ShowDialogCustom(object content, string? caption = null, DialogMode mode = DialogMode.None);

        void ShowDialogProgress(string message, string? caption = null, DialogMode mode = DialogMode.None, bool indeterminate = true);

        TaskbarItemInfo TaskbarItemInfo { get; set; }
    }

    public struct TaskbarItemInfo
    {
        public string Description { get; set; }

        public TaskbarItemProgressState ProgressState { get; set; }

        public double ProgressValue { get; set; }
    }

    public enum TaskbarItemProgressState
    {
        // Summary:
        //     No progress indicator is displayed in the taskbar button.
        None = 0,

        // Summary:
        //     A pulsing green indicator is displayed in the taskbar button.
        Indeterminate = 1,

        // Summary:
        //     A green progress indicator is displayed in the taskbar button.
        Normal = 2,

        // Summary:
        //     A red progress indicator is displayed in the taskbar button.
        Error = 3,

        // Summary:
        //     A yellow progress indicator is displayed in the taskbar button.
        Paused = 4,
    }
}
