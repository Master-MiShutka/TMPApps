using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TMPApplication
{
    partial class TMPApp
    {
        BindingErrorListener errorListener;
        /// <summary>
        /// Запуск отслеживания ошибок привязки
        /// </summary>
        public void AttachBindingErrorListener()
        {
            errorListener = new BindingErrorListener();
            errorListener.ErrorCatched += OnBindingErrorCatched;
        }
        /// <summary>
        /// Прекращение отслеживания ошибок привязки
        /// </summary>
        public void DetachBindingErrorListener()
        {
            if (IsAttached)
            {
                errorListener.ErrorCatched -= OnBindingErrorCatched;
                errorListener.Dispose();
                errorListener = null;
            }
        }
        [DebuggerStepThrough]
        void OnBindingErrorCatched(string message)
        {
            LogError(message);
            //throw new BindingException(message);
        }
        /// <summary>
        /// Включено ли отслеживание ошибок привязки
        /// </summary>
        public bool IsAttached
        {
            get { return errorListener != null; }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowError(e.ExceptionObject);
        }

        private static void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(TMPApp.GetExceptionDetails(e.Exception));
#endif
            LogException(e.Exception);
            if (e.Exception.GetType() == typeof(System.IO.FileNotFoundException))
            {
                ShowError("Не удалось найти файл.\n" + e.Exception.Message + "\nПопробуйте переустановить приложение.");
            }
            else if (e.Exception.InnerException != null)
            {
                ShowError(e.Exception.InnerException);
            }
            else
            {
                ShowError(e.Exception);
            }
            e.Handled = true;
        }

        public static string GetExceptionDetails(Exception exp)
        {
            string message = string.Empty;
            try
            {
                // Write Message tree of inner exception into textual representation
                message = exp.Message;

                Exception innerEx = exp.InnerException;

                for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                {
                    string spaces = string.Empty;

                    for (int j = 0; j < i; j++)
                        spaces += "  ";

                    message += "\n" + spaces + "└─>" + innerEx.Message;
                }
            }
            catch { }
            return message;
        }
    }
}
