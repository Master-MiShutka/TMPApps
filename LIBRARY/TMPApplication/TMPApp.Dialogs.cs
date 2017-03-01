using System;
using System.Windows;
using System.Windows.Threading;

namespace TMPApplication
{
    partial class TMPApp
    {
        public static WpfDialogs.Contracts.IDialogManager DialogManager { get; private set; }

        public static void InitDialogs()
        {
            if (Current == null || Current.MainWindow == null)
                throw new ArgumentNullException();
            if ((Current.MainWindow as WpfDialogs.Contracts.IDialogWindow) == null)
                throw new ArgumentException("MainWindow is not a WpfDialogs.Contracts.IDialogWindow");

            DialogManager = new WpfDialogs.DialogManager((Current.MainWindow as WpfDialogs.Contracts.IDialogWindow).DialogLayer, 
                Current.MainWindow.Dispatcher);

            if (DialogManager == null)
                throw new ArgumentNullException("DialogManager == null");
        }

        /// <summary>
        /// Show an error dialog for the user.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public static void ShowError(object exception)
        {
            Exception e = (Exception)exception;
            if (e != null)
                ShowError(e, null);
            else
                ShowError("Произошла неисправимая ошибка.\nПрограмма будет закрыта.");
        }
        public static MessageBoxResult ShowError(string message)
        {
            Func<MessageBoxResult> func = () =>
            {
                LogError("ОШИБКА: " + message);
                System.Media.SystemSounds.Exclamation.Play();
                if (Current == null || Current.MainWindow == null)
                    return MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    return MessageBox.Show(Current.MainWindow, message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        public static MessageBoxResult ShowError(Exception e, string format)
        {
            Func<MessageBoxResult> func = () =>
            {
                if (String.IsNullOrEmpty(format))
                    format = "Произошла ошибка.\nОписание ошибки:\n{0}";
                System.Media.SystemSounds.Exclamation.Play();
                string msg = String.Format(format, GetExceptionDetails(e));
                LogError(msg + "\nТрассировка:\n" + e.StackTrace);

                if (Current == null || Current.MainWindow == null)
                    return MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    return MessageBox.Show(Current.MainWindow, msg, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        public static MessageBoxResult ShowWarning(string message)
        {
            Func<MessageBoxResult> func = () =>
            {
                Log("ВНИМАНИЕ: " + message);
                System.Media.SystemSounds.Hand.Play();
                return MessageBox.Show(Current.MainWindow, message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        public static MessageBoxResult ShowInfo(string message, Action onOk = null)
        {
            Func<MessageBoxResult> func = () =>
            {
                Log("ИНФО: " + message);
                System.Media.SystemSounds.Asterisk.Play();

                var dialog = MessageDialog(message, "Сообщение");
                dialog.Ok = onOk;

                return MessageBox.Show(Current.MainWindow, message, Title, MessageBoxButton.OK, MessageBoxImage.Information);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        public static MessageBoxResult ShowQuestion(string message)
        {
            Func<MessageBoxResult> func = () =>
            {
                System.Media.SystemSounds.Question.Play();
                return MessageBox.Show(Current.MainWindow, message, Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }

        #region Dialogs

        public static WpfDialogs.Contracts.IDialog MessageDialog(string message, string caption = null,
            System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.None,
            WpfDialogs.DialogMode mode = WpfDialogs.DialogMode.Ok)
        {
            if (DialogManager == null)
                InitDialogs();

            switch (image)
            {
                case MessageBoxImage.None:
                    System.Media.SystemSounds.Beep.Play();
                    break;
                case MessageBoxImage.Error:
                    System.Media.SystemSounds.Exclamation.Play();
                    break;
                case MessageBoxImage.Warning:
                    System.Media.SystemSounds.Hand.Play();
                    break;
                case MessageBoxImage.Information:
                    System.Media.SystemSounds.Asterisk.Play();
                    break;
                case MessageBoxImage.Question:
                    System.Media.SystemSounds.Question.Play();
                    break;
            }            

            var dialog = DialogManager.CreateMessageDialog(message, caption, mode, image);
            return dialog;
        }
        public static WpfDialogs.Contracts.IDialog ShowErrorDialog(string message, string caption = null, Action ok = null)
        {
            LogError(message);

            if (DialogManager == null)
                InitDialogs();

            System.Media.SystemSounds.Exclamation.Play();

            if (String.IsNullOrEmpty(caption))
                caption = Title;

            var dialog = DialogManager.CreateMessageDialog(message, caption, WpfDialogs.DialogMode.Ok, System.Windows.MessageBoxImage.Error);
            dialog.Ok = ok;
            dialog.Show();
            return dialog;
        }
        public static WpfDialogs.Contracts.IDialog ShowErrorDialog(Exception e)
        {
            return ShowErrorDialog(GetExceptionDetails(e));
        }
        public static WpfDialogs.Contracts.IDialog ShowErrorDialog(Exception e, string format)
        {
            return ShowErrorDialog(String.Format(format, GetExceptionDetails(e)));
        }
        public static WpfDialogs.Contracts.IDialog ShowWarningDialog(string message, string caption = null, Action ok = null)
        {
            LogWarning(message);

            if (DialogManager == null)
                InitDialogs();

            System.Media.SystemSounds.Hand.Play();

            if (String.IsNullOrEmpty(caption))
                caption = Title;

            var dialog = DialogManager.CreateMessageDialog(message, caption, WpfDialogs.DialogMode.Ok, System.Windows.MessageBoxImage.Warning);
            dialog.Ok = ok;
            dialog.Show();
            return dialog;
        }
        public static WpfDialogs.Contracts.IDialog ShowInfoDialog(string message, string caption = null, Action ok = null)
        {
            if (DialogManager == null)
                InitDialogs();

            System.Media.SystemSounds.Asterisk.Play();

            if (String.IsNullOrEmpty(caption))
                caption = Title;

            var dialog = DialogManager.CreateMessageDialog(message, caption, WpfDialogs.DialogMode.Ok, System.Windows.MessageBoxImage.Information);
            dialog.Ok = ok;
            dialog.Show();
            return dialog;
        }
        public static WpfDialogs.Contracts.IDialog QuestionDialog(string message, string caption = null, WpfDialogs.DialogMode mode = WpfDialogs.DialogMode.YesNo)
        {
            if (DialogManager == null)
                InitDialogs();

            System.Media.SystemSounds.Question.Play();

            if (String.IsNullOrEmpty(caption))
                caption = Title;

            var dialog = DialogManager.CreateMessageDialog(message, caption, mode, System.Windows.MessageBoxImage.Question);
            return dialog;
        }

        public static WpfDialogs.Contracts.IDialog WaitingScreen(string message, bool indeterminate = true, WpfDialogs.DialogMode mode = WpfDialogs.DialogMode.None)
        {
            if (DialogManager == null)
                InitDialogs();

            var dialog = DialogManager.CreateWaitDialog(message, mode, indeterminate);
            return dialog;
        }
        public static WpfDialogs.Contracts.IDialog Dialog(System.Windows.Controls.Control content, WpfDialogs.DialogMode mode = WpfDialogs.DialogMode.None)
        {
            if (DialogManager == null)
                InitDialogs();

            var dialog = DialogManager.CreateCustomContentDialog(content, mode);
            return dialog;
        }
        public static WpfDialogs.Contracts.IDialog ProgressDialog(string message, Action action,
            WpfDialogs.DialogMode mode = WpfDialogs.DialogMode.None, bool indeterminate = true)
        {
            if (DialogManager == null)
                InitDialogs();

            var dialog = DialogManager.CreateProgressDialog(message, mode, indeterminate);
            return dialog;
        }

        public static void UIAction(Action action)
        {
            DispatcherExtensions.InUi(action);
        }

        #endregion
    }
}