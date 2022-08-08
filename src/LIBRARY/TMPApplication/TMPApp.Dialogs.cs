namespace TMPApplication
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    partial class TMPApp
    {
        public Window CreateExternalWindow(
            bool showInTaskbar = false,
            bool showActivated = true,
            bool topmost = true,
            ResizeMode resizeMode = ResizeMode.NoResize,
            WindowStyle windowStyle = WindowStyle.None,
            WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen,
            bool showTitleBar = false,

            bool showMinButton = false,
            bool showMaxButton = false,
            bool showCloseButton = false)
        {
            return new CustomWpfWindow.WindowWithDialogs
            {
                ShowInTaskbar = showInTaskbar,
                ShowActivated = showActivated,
                Topmost = topmost,
                ResizeMode = resizeMode,
                WindowStyle = windowStyle,
                WindowStartupLocation = windowStartupLocation,
                ShowTitleBar = showTitleBar,

                ShowMinButton = showMinButton,
                ShowMaxRestoreButton = showMaxButton,
                ShowCloseButton = showCloseButton,
            };
        }

        /// <summary>
        /// Отобразить сообщение об ошибке при возникновении исключения
        /// </summary>
        /// <param name="exception">
        /// Исключение
        /// </param>
        public static void ShowError(object exception, Action runAfter = null)
        {
            Exception e = (Exception)exception;
            if (e != null)
            {
                ShowError(exception: e, null, runAfter: runAfter);
            }
            else
            {
                ShowError(message: "Произошла неисправимая ошибка.\nПрограмма будет закрыта.", runAfter: runAfter);
            }
        }

        /// <summary>
        /// Отобразить сообщение об ошибке
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowError(string message, MessageBoxButton buttons = MessageBoxButton.OK, Action runAfter = null)
        {
            Func<MessageBoxResult> func = () =>
            {
                System.Media.SystemSounds.Exclamation.Play();
                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Error("ОШИБКА: " + message);
                    return MessageBox.Show(message, Instance?.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is WpfDialogs.Contracts.IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        windowWithDialogs.ShowDialogError(message, Instance.Title, ok: runAfter);
                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Error("ОШИБКА: " + message);
                        return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };
            if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() == false)
            {
                return (MessageBoxResult)DispatcherExtensions.InUi(func);
            }
            else
            {
                return func();
            }
        }

        /// <summary>
        /// Отобразить сообщение об ошибке при возникновении исключения указанного формата
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <param name="format">Формат сообщения</param>
        /// <returns></returns>
        public static MessageBoxResult ShowError(Exception exception, string format, MessageBoxButton buttons = MessageBoxButton.OK, Action runAfter = null)
        {
            Func<MessageBoxResult> func = () =>
            {
                if (string.IsNullOrEmpty(format))
                {
                    format = "Произошла ошибка.\nОписание ошибки:\n{0}";
                }

                System.Media.SystemSounds.Exclamation.Play();
                string msg = string.Format(format, GetExceptionDetails(exception));

                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Error(message: msg + "\nТрассировка:\n" + exception.StackTrace);
                    return MessageBox.Show(msg, Instance.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is WpfDialogs.Contracts.IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        windowWithDialogs.ShowDialogError(msg, Instance.Title, ok: runAfter);
                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Error(msg + "\nТрассировка:\n" + exception.StackTrace);
                        return MessageBox.Show(Current.MainWindow, msg, Instance.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };

            if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() == false)
            {
                return (MessageBoxResult)DispatcherExtensions.InUi(func);
            }
            else
            {
                return func();
            }
        }

        /// <summary>
        /// Отобразить предупреждающее сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowWarning(string message, MessageBoxButton buttons = MessageBoxButton.OK, Action runAfter = null)
        {
            System.Media.SystemSounds.Hand.Play();

            Func<MessageBoxResult> func = () =>
            {
                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Warn("ВНИМАНИЕ: " + message);
                    return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is WpfDialogs.Contracts.IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        windowWithDialogs.ShowDialogWarning(message, Instance.Title, ok: runAfter);
                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Warn("ВНИМАНИЕ: " + message);
                        return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }

        /// <summary>
        /// Отобразить информационное сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowInfo(string message, Action onOk, MessageBoxButton buttons = MessageBoxButton.OK, Action runAfter = null)
        {
            Func<MessageBoxResult> func = () =>
            {
                System.Media.SystemSounds.Asterisk.Play();

                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Info("ИНФО: " + message);
                    return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is WpfDialogs.Contracts.IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        var dialog = windowWithDialogs.DialogInfo(message, Instance.Title, ok: runAfter);

                        dialog.Ok = onOk;
                        dialog.Show();

                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Info("ИНФО: " + message);
                        return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }

        /// <summary>
        /// Отобразить сообщение с вопросом
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static void ShowQuestion(string message, Action onYes, Action onNo, MessageBoxButton buttons = MessageBoxButton.YesNo)
        {
            Action func = () =>
            {
                System.Media.SystemSounds.Question.Play();
                if (Current == null || Current.MainWindow == null)
                {
                    var result = MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == MessageBoxResult.Yes)
                        onYes?.Invoke();
                    if (result == MessageBoxResult.No)
                        onNo?.Invoke();
                }
                else
                {
                    if (Current.MainWindow is WpfDialogs.Contracts.IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        var dialog = windowWithDialogs.DialogQuestion(message, Instance.Title);

                        dialog.Yes = onYes;
                        dialog.No = onNo;

                        dialog.Show();
                    }
                    else
                    {
                        var result = MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == MessageBoxResult.Yes)
                            onYes?.Invoke();
                        if (result == MessageBoxResult.No)
                            onNo?.Invoke();
                    }
                }
            };
            DispatcherExtensions.InUi(func);
        }
    }
}
