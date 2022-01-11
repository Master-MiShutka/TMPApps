using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TMPApplication;
using TMPApplication.CustomWpfWindow;
using TMPApplication.WpfDialogs;
using TMPApplication.WpfDialogs.Contracts;

namespace TMP.Work.Emcos.ViewModel
{
    public abstract class BaseViewModel : PropertyChangedBase
    {
        #region Fields

#if DEBUG
        protected readonly Action<Exception> _callBackAction = (e) => System.Diagnostics.Debug.Fail(EmcosSiteWrapperApp.GetExceptionDetails(e));
#else
        protected readonly Action<Exception> _callBackAction = EmcosSiteWrapperApp.LogException;
#endif

        protected System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();

        protected DateTime _operationStartDateTime;
        protected readonly Timer _timer = new Timer(1000d) { AutoReset = true };

        protected State _state = State.Busy;

        #endregion
        

        public BaseViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
                return;

            Window = EmcosSiteWrapperApp.Current.MainWindow as IWindowWithDialogs;
        }

        public BaseViewModel(IWindowWithDialogs window)
        {
            Window = window;
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(window as System.Windows.DependencyObject))
                return;
            Window = window ?? throw new ArgumentNullException("Window");
        }
        /// <summary>
        /// Окно, реализующее интерфейс <see cref="IWindowWithDialogs"/>, к которому привязан этот контроллер
        /// </summary>
        public IWindowWithDialogs Window { get; private set; }
        /// <summary>
        /// Сообщение для диалога ожидания
        /// </summary>
        public string DialogMessage { get; set; }
        /// <summary>
        /// Ссылка на диалог
        /// </summary>
        public IDialog Dialog { get; private set; }
        /// <summary>
        /// Статус контроллера
        /// </summary>
        public State Status
        {
            get { return _state; }
            set
            {
                SetProp(ref _state, value, "Status");
                if (_state == State.Busy)
                {
                    ShowDialogWaitingScreen(DialogMessage);
                }
                else
                    if (_state == State.Fail)
                    {
                        try
                        {
                            (Window as System.Windows.Window).TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        }
                        catch { }
                    }
                    else
                        CloseDialog();
            }
        }

        #region Dialogs

        public IDialog ShowDialogError(string message, string caption = null, Action ok = null)
        {
            Status = State.Fail;
            IDialog dialog = null;
            if (ok == null)
                ok = () =>
                    Status = State.Ready;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogError(message, caption, ok);
                dialog.Show();
            });
            return dialog;
        }
        public IDialog ShowDialogError(Exception e)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogError(e);
                dialog.Show();
            });
            return dialog;
        }
        public IDialog ShowDialogError(Exception e, string format)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogError(e, format);
                dialog.Show();
            });
            return dialog;
        }
        public IDialog ShowDialogWarning(string message, string caption = null, Action ok = null)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogWarning(message, caption, ok);
                dialog.Show();
            });
            return dialog;
        }
        public IDialog ShowDialogInfo(string message)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogInfo(message);
                dialog.Show();
            });
            return dialog;
        }
        public IDialog ShowDialogInfo(string message, string caption = null, Action ok = null)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogInfo(message, caption, ok);
                dialog.Show();
            });
            return dialog;
        }
        public IDialog ShowDialogInfo(string message, string caption = null, System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.None,
            DialogMode mode = DialogMode.Ok)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogInfo(message, caption, image, mode);
                dialog.Show();
            });
            return dialog;
        }

        public IDialog ShowDialogQuestion(string message, string caption = null, DialogMode mode = DialogMode.YesNo)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogQuestion(message, caption, mode);
                dialog.Show();
            });
            return dialog;
        }

        public void ShowDialogWaitingScreen(string message, bool indeterminate = true, DialogMode mode = DialogMode.None)
        {
            DispatcherExtensions.InUi(() =>
            {
                (Window as System.Windows.Window).TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                Dialog = Window.DialogWaitingScreen(message, indeterminate, mode);
                Dialog.Show();
            });
        }
        public IDialog ShowDialogCustom(System.Windows.Controls.Control content, DialogMode mode = DialogMode.None)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogCustom(content, mode);
                dialog.Show();
            });
            return dialog;
        }
        public IDialog ShowDialogProgress(string message, DialogMode mode = DialogMode.None, bool indeterminate = true)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogProgress(message, mode, indeterminate);
                dialog.Show();
            });
            return dialog;
        }

        public IDialog CreateDialogCustom(System.Windows.Controls.Control content,
            DialogMode mode = DialogMode.None)
        {
            IDialog dialog = null;
            DispatcherExtensions.InUi(() =>
            {
                dialog = Window.DialogCustom(content, mode);
            });
            return dialog;
        }

        #endregion

        /// <summary>
        /// Закрытие последнего диалога
        /// </summary>
        public void CloseDialog()
        {
            if (Dialog != null)
            {
                DispatcherExtensions.InUi(() =>
                {
                    Dialog.Close();
                    (Window as System.Windows.Window).TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                });
                Dialog = null;
                DialogMessage = null;
            }
        }

        public enum State
        {
            [Description("Ожидание команды")]
            Ready,
            [Description("Выполнение задачи")]
            Busy,
            [Description("Возникла ошибка")]
            Fail
        }
    }
}
