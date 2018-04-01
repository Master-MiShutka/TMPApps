using System;
using System.Windows;
using System.Windows.Threading;

namespace TMPApplication
{
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
                ShowCloseButton = showCloseButton
            };
        }

        /// <summary>
        /// Отобразить сообщение об ошибке при возникновении исключения
        /// </summary>
        /// <param name="exception">
        /// Исключение
        /// </param>
        public static void ShowError(object exception)
        {
            Exception e = (Exception)exception;
            if (e != null)
                ShowError(e, null);
            else
                ShowError("Произошла неисправимая ошибка.\nПрограмма будет закрыта.");
        }
        /// <summary>
        /// Отобразить сообщение об ошибке
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowError(string message, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            Func<MessageBoxResult> func = () =>
            {
                LogError("ОШИБКА: " + message);
                System.Media.SystemSounds.Exclamation.Play();
                if (Current == null || Current.MainWindow == null)
                    return MessageBox.Show(message, Title, buttons, MessageBoxImage.Error);
                else
                    return MessageBox.Show(Current.MainWindow, message, Title, buttons, MessageBoxImage.Error);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        /// <summary>
        /// Отобразить сообщение об ошибке при возникновении исключения указанного формата
        /// </summary>
        /// <param name="e">Исключение</param>
        /// <param name="format">Формат сообщения</param>
        /// <returns></returns>
        public static MessageBoxResult ShowError(Exception e, string format, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            Func<MessageBoxResult> func = () =>
            {
                if (String.IsNullOrEmpty(format))
                    format = "Произошла ошибка.\nОписание ошибки:\n{0}";
                System.Media.SystemSounds.Exclamation.Play();
                string msg = String.Format(format, GetExceptionDetails(e));
                LogError(msg + "\nТрассировка:\n" + e.StackTrace);

                if (Current == null || Current.MainWindow == null)
                    return MessageBox.Show(msg, Title, buttons, MessageBoxImage.Error);
                else
                    return MessageBox.Show(Current.MainWindow, msg, Title, buttons, MessageBoxImage.Error);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        /// <summary>
        /// Отобразить предупреждающее сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowWarning(string message, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            Func<MessageBoxResult> func = () =>
            {
                Log("ВНИМАНИЕ: " + message);
                System.Media.SystemSounds.Hand.Play();
                return MessageBox.Show(Current.MainWindow, message, Title, buttons, MessageBoxImage.Warning);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        /// <summary>
        /// Отобразить информационное сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowInfo(string message, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            Func<MessageBoxResult> func = () =>
            {
                Log("ИНФО: " + message);
                System.Media.SystemSounds.Asterisk.Play();

                return MessageBox.Show(Current.MainWindow, message, Title, buttons, MessageBoxImage.Information);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        /// <summary>
        /// Отобразить сообщение с вопросом
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowQuestion(string message, MessageBoxButton buttons = MessageBoxButton.YesNo)
        {
            Func<MessageBoxResult> func = () =>
            {
                System.Media.SystemSounds.Question.Play();
                return MessageBox.Show(Current.MainWindow, message, Title, buttons, MessageBoxImage.Question);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
    }
}