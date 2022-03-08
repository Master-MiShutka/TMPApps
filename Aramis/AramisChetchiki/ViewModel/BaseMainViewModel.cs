namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Input;
    using TMP.Shared;
    using TMP.Shared.Commands;

    [DataContract]
    public abstract class BaseMainViewModel : PropertyChangedBase, IDisposable
    {
        private bool isDisposed;
        private Window window;
        private readonly TMPApplication.WpfDialogs.Contracts.IWindowWithDialogs mainWindow = TMPApplication.TMPApp.Instance?.MainWindowWithDialogs;

        private bool isAnalizingData;
        private string status;
        private string detailedStatus;
        private bool isBusy;

        private TMPApplication.WpfDialogs.Contracts.IDialog busyDialog;
        private System.Threading.CancellationTokenSource isBusyCancellationTokenSource = new System.Threading.CancellationTokenSource();

        protected BaseMainViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            TMPApplication.DispatcherExtensions.InUi(() => this.window = App.Current.MainWindow);
            if (this.window == null)
            {
                throw new ArgumentNullException("App.Current.MainWindow");
            }

            this.CommandCloseWindow = new DelegateCommand(this.DoCloseWindow);

            this.window.Closed += this.MainWindow_Closed;
        }

        /// <summary>
        /// Для прерывания операции
        /// </summary>
        public System.Threading.CancellationTokenSource IsBusyCancellationTokenSource => this.isBusyCancellationTokenSource;

        /// <summary>
        /// Статус, при присвоении пустой строки или null сбросит флаг IsBusy
        /// </summary>
        public virtual string Status
        {
            get => this.status;
            set
            {
                if (this.SetProperty(ref this.status, value))
                {
                    if (string.IsNullOrEmpty(this.status))
                    {
                        this.IsBusy = false;
                    }
                }
            }
        }

        /// <summary>
        /// Подробный статус
        /// </summary>
        public virtual string DetailedStatus
        {
            get => this.detailedStatus;
            set => this.SetProperty(ref this.detailedStatus, value);
        }

        /// <summary>
        /// Признак, указывающий, что выполняется длительная операция
        /// Автоматически спустя 100мс отобразится окно "ожидания" с текстом
        /// из свойства Status
        /// </summary>
        public virtual bool IsBusy
        {
            get => this.isBusy;
            set
            {
                this.SetProperty(ref this.isBusy, value);
                if (this.isBusy)
                {
                    System.Threading.CancellationTokenSource cts = this.isBusyCancellationTokenSource = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
                    this.RaisePropertyChanged(nameof(this.IsBusyCancellationTokenSource));
                    System.Threading.Tasks.Task waitTask = System.Threading.Tasks.Task.Run(
                        () =>
                        {
                            System.Threading.Thread.Sleep(100);

                            if (this.isBusy && this.isBusyCancellationTokenSource?.IsCancellationRequested == false)
                            {
                                TMPApplication.TMPApp.InvokeInUIThread(() =>
                                {
                                    if (this.isBusyCancellationTokenSource != null)
                                    {
                                        this.busyDialog = this.mainWindow.DialogWaitingScreen(this.Status);
                                        this.busyDialog.Cancel = () =>
                                        {
                                            this.isBusyCancellationTokenSource?.Cancel();

                                            this.busyDialog.CanCancel = false;
                                            this.busyDialog.CancelText = "отменяется ...";
                                        };
                                        this.busyDialog.Show();
                                        return;
                                    }
                                });
                            }

                            this.isBusyCancellationTokenSource = null;
                            this.RaisePropertyChanged(nameof(this.IsBusyCancellationTokenSource));
                        }, cts.Token);
                }
                else
                {
                    this.isBusyCancellationTokenSource?.Cancel();
                    if (this.busyDialog != null)
                    {
                        this.busyDialog.Close();
                        this.busyDialog = null;
                    }
                }
            }
        }

        /// <summary>
        /// Признак, указывающий, что выполняется анализ информации
        /// </summary>
        public bool IsAnalizingData
        {
            get => this.isAnalizingData;
            set => this.SetProperty(ref this.isAnalizingData, value);
        }

        /// <summary>
        /// Команда для закрытия активного окна
        /// </summary>
        public ICommand CommandCloseWindow { get; }

        #region IDisposable implementation

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.OnDispose();

                // free managed resources
                if (this.isBusyCancellationTokenSource != null)
                {
                    this.isBusyCancellationTokenSource.Dispose();
                }

                if (this.window != null)
                {
                    this.window.Closed -= this.MainWindow_Closed;
                    this.window.Closed -= this.MainWindow_Closed;
                }
            }

            this.isDisposed = true;
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~BaseMainViewModel()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        protected virtual void OnDispose()
        {
        }

        #endregion

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            this.OnClosingMainWindow();
        }

        /// <summary>
        /// Вызывается при закрытии главного окна приложения
        /// </summary>
        protected virtual void OnClosingMainWindow()
        {
        }

        protected void SetWindowTaskbarItemProgressState(TMPApplication.WpfDialogs.Contracts.TaskbarItemProgressState taskbarItemProgressState, double value = -1)
        {
            TMPApplication.DispatcherExtensions.InUi(() =>
            {
                this.mainWindow.TaskbarItemInfo = value != -1
                    ? new TMPApplication.WpfDialogs.Contracts.TaskbarItemInfo
                    {
                        ProgressState = taskbarItemProgressState,
                        ProgressValue = value,
                    }
                    : new TMPApplication.WpfDialogs.Contracts.TaskbarItemInfo
                    {
                        ProgressState = taskbarItemProgressState,
                    };
            });
        }

        public void ShowDialogError(Exception exception)
        {
            this.mainWindow.ShowDialogError(exception);
        }

        public void ShowDialogError(string message)
        {
            this.mainWindow.ShowDialogError(message);
        }

        public void ShowDialogInfo(string message)
        {
            this.mainWindow.ShowDialogInfo(message, caption: string.Empty);
        }

        public void ShowDialogWarning(string message)
        {
            this.mainWindow.ShowDialogWarning(message, caption: string.Empty);
        }

        public void ShowDialogQuestion(string message)
        {
            this.mainWindow.ShowDialogQuestion(message);
        }

        public Action ShowCustomDialog(System.Windows.Controls.Control control, string title, TMPApplication.WpfDialogs.DialogMode dialogMode = TMPApplication.WpfDialogs.DialogMode.Ok)
        {
            TMPApplication.WpfDialogs.Contracts.ICustomContentDialog dialog = null;
            dialog = this.mainWindow.DialogCustom(
                control,
                title,
                mode: dialogMode);

            void dialogCloseAction()
            {
                dialog.Close();
            }

            dialog.Show();

            return dialogCloseAction;
        }

        public override int GetHashCode()
        {
            Guid guid = new Guid("1E3F77FC-9833-46F3-AB7E-A991707B1CB9");
            return guid.GetHashCode();
        }

        private void DoCloseWindow()
        {
            this.window.Close();
        }
    }
}
