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
        /// <summary>
        /// Отобразить сообщение об ошибке при возникновении исключения указанного формата
        /// </summary>
        /// <param name="e">Исключение</param>
        /// <param name="format">Формат сообщения</param>
        /// <returns></returns>
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
        /// <summary>
        /// Отобразить предупреждающее сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
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
        /// <summary>
        /// Отобразить информационное сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowInfo(string message)
        {
            Func<MessageBoxResult> func = () =>
            {
                Log("ИНФО: " + message);
                System.Media.SystemSounds.Asterisk.Play();

                return MessageBox.Show(Current.MainWindow, message, Title, MessageBoxButton.OK, MessageBoxImage.Information);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
        /// <summary>
        /// Отобразить сообщение с вопросом
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowQuestion(string message)
        {
            Func<MessageBoxResult> func = () =>
            {
                System.Media.SystemSounds.Question.Play();
                return MessageBox.Show(Current.MainWindow, message, Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }
    }
}