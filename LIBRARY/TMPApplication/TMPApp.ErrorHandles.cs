namespace TMPApplication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows;

    partial class TMPApp
    {
        private BindingErrorListener errorListener;

        /// <summary>
        /// Запуск отслеживания ошибок привязки
        /// </summary>
        public void AttachBindingErrorListener()
        {
            this.errorListener = new BindingErrorListener();
            this.errorListener.ErrorCatched += this.OnBindingErrorCatched;
        }

        /// <summary>
        /// Прекращение отслеживания ошибок привязки
        /// </summary>
        public void DetachBindingErrorListener()
        {
            if (this.IsAttached)
            {
                this.errorListener.ErrorCatched -= this.OnBindingErrorCatched;
                this.errorListener.Dispose();
                this.errorListener = null;
            }
        }

        [DebuggerStepThrough]
        private void OnBindingErrorCatched(string message)
        {
            logger?.Error(message);

            // throw new BindingException(message);
        }

        /// <summary>
        /// Включено ли отслеживание ошибок привязки
        /// </summary>
        public bool IsAttached => this.errorListener != null;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowError(e.ExceptionObject);
        }

        private static void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            TMPApp app = Application.Current as TMPApp;
#if DEBUG
            System.Diagnostics.Debug.WriteLine(TMPApp.GetExceptionDetails(e.Exception));
#endif
            logger?.Error(e.Exception);
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
            if (exp is AggregateException)
            {
                AggregateException ae = exp as AggregateException;
                foreach (var e in ae.InnerExceptions)
                {
                    message += Environment.NewLine + e.Message + Environment.NewLine;
                }

                ParseException(ae.InnerExceptions.LastOrDefault());
            }
            else
            {
                ParseException(exp);
            }

            message += "\n" + BuildStackTrace(exp);

            void ParseException(Exception e)
            {
                try
                {
                    // Write Message tree of inner exception into textual representation
                    message = e.Message;

                    Exception innerEx = e.InnerException;

                    for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                    {
                        string spaces = string.Empty;

                        for (int j = 0; j < i; j++)
                        {
                            spaces += "  ";
                        }

                        message += "\n" + spaces + "└─>" + innerEx.Message;
                    }
                }
                catch
                {
                }
            }

            return message;
        }

        public static string BuildStackTrace(Exception e)
        {
            var st = new StackTrace(e, true);
            var frames = st.GetFrames();
            var traceString = new StringBuilder();

            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                traceString.Append("File: " + frame.GetFileName());
                traceString.Append(", Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
            }

            return traceString.ToString();
        }
    }
}
