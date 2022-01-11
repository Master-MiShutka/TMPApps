namespace TMP.Shared.Commands
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>
    /// Асинхронная команда, исполняющаяся в потоке из пула потоков
    /// </summary>
    public class AsynchronousDelegateCommand : DelegateCommand
    {
        #region Fields

        private DelegateCommand cancelCommand;
        protected Dispatcher callingDispatcher;
        private bool isExecuting = false;
        private bool isCancellationRequested;

        private Action onCompleted;
        private Action onCanceled;

        #endregion
        #region Constructors

        /// <summary>
        /// Создание нового экземпляра класса <see cref="AsynchronousDelegateCommand"/>
        /// </summary>
        /// <param name="action">Действие</param>
        /// <param name="canExecute">если возвращает <c>true</c>, разрешено выполнение</param>
        public AsynchronousDelegateCommand(Action action, Predicate<object> canExecute) : base(action, canExecute)
        {
            // инициализация
            this.Initialise();
        }

        public AsynchronousDelegateCommand(Action action, Predicate<object> canExecute, string header) : base(action, canExecute, header)
        {
            // инициализация
            this.Initialise();
        }

        /// <summary>
        /// Создание нового экземпляра класса <see cref="AsynchronousDelegateCommand"/>
        /// </summary>
        public AsynchronousDelegateCommand(Action action, Predicate<object> canExecute, Action completed, Action canceled = null)
            : base(action, canExecute)
        {
            this.onCompleted = completed;
            this.onCanceled = canceled;

            // инициализация
            this.Initialise();
        }

        /// <summary>
        /// Создание нового экземпляра класса <see cref="AsynchronousDelegateCommand"/>
        /// </summary>
        /// <param name="parameterizedAction">Действие с параметром</param>
        /// <param name="canExecute">если возвращает <c>true</c>, разрешено выполнение</param>
        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute)
            : base(parameterizedAction, canExecute)
        {
            // инициализация
            this.Initialise();
        }

        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, string header)
            : base(parameterizedAction, canExecute, header)
        {
            // инициализация
            this.Initialise();
        }

        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, Func<string> getHeader)
            : base(parameterizedAction, canExecute, getHeader)
        {
            // инициализация
            this.Initialise();
        }

        /// <summary>
        /// Создание нового экземпляра класса <see cref="AsynchronousDelegateCommand"/>
        /// </summary>
        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, Action completed, Action canceled = null)
            : base(parameterizedAction, canExecute)
        {
            this.onCompleted = completed;
            this.onCanceled = canceled;

            // инициализация
            this.Initialise();
        }

        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, Func<string> getHeader, Action completed, Action canceled = null)
            : this(parameterizedAction, canExecute, completed, canceled)
        {
            this.getHeaderFunc = getHeader;
        }
        #endregion

        /// <summary>
        /// инициализация
        /// </summary>
        private void Initialise()
        {
            // создание команды отмены
            this.cancelCommand = new DelegateCommand(
              () =>
              {
                  // установка флага
                  this.IsCancellationRequested = true;
              });
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="param">Параметр</param>
        public override void DoExecute(object param)
        {
            // Уже исполняется
            if (this.IsExecuting)
            {
                return;
            }

            // Вызов команды
            CancelCommandEventArgs args = new CancelCommandEventArgs() { Parameter = param, Cancel = false };
            this.InvokeExecuting(args);
            param = args.Parameter;

            // Отменена?
            if (args.Cancel)
            {
                return;
            }

            // Выполняется
            this.IsExecuting = true;

            // Сохранение диспетчера потока
            this.callingDispatcher = Dispatcher.CurrentDispatcher;

            // Запуск действия в новом потоке из пула
            ThreadPool.QueueUserWorkItem(
              (state) =>
              {
                  this.InvokeAction(param);

                  // вызов событий и установка состояния
                  this.ReportProgress(
                  () =>
                    {
                        // выполнение заверешено
                        this.IsExecuting = false;

                        // если выполнение отменено, вызов события отмены, иначе события исполнения
                        if (this.IsCancellationRequested)
                        {
                            this.InvokeCancelled(new CommandEventArgs() { Parameter = param });
                            if (this.onCanceled != null)
                            {
                                this.onCanceled();
                            }
                        }
                        else
                        {
                            this.InvokeExecuted(new CommandEventArgs() { Parameter = param });
                            if (this.onCompleted != null)
                            {
                                this.onCompleted();
                            }
                        }

                        // сброс флага
                        this.IsCancellationRequested = false;
                    });
              });
        }

        private void ReportProgress(Action action)
        {
            if (this.IsExecuting)
            {
                if (this.callingDispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    this.callingDispatcher.BeginInvoke((Action)(() => { action(); }));
                }
            }
        }
        #region Properties

        /// <summary>
        /// Отмена выполнения, если был запрос
        /// </summary>
        /// <returns>True если команда была отменена</returns>
        public bool CancelIfRequested()
        {
            if (this.IsCancellationRequested == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Возвращает или устанавливает значение, определяющее выполняется ли команда
        /// </summary>
        /// <value>
        /// <c>true</c> если исполняется; иначе, <c>false</c>.
        /// </value>
        public bool IsExecuting
        {
            get => this.isExecuting;

            set => this.SetProperty(ref this.isExecuting, value, nameof(this.IsExecuting));
        }

        /// <summary>
        /// Возвращает или устанавливает значение, сообщающее запросил ли этот экземпляр отмену команды
        /// </summary>
        /// <value>
        /// <c>true</c> если этот экземпляр запросил отмену команды; иначе, <c>false</c>.
        /// </value>
        public bool IsCancellationRequested
        {
            get => this.isCancellationRequested;

            set => this.SetProperty(ref this.isCancellationRequested, value, nameof(this.IsCancellationRequested));
        }

        /// <summary>
        /// Возвращает команду отмены
        /// </summary>
        public DelegateCommand CancelCommand => this.cancelCommand;
        #endregion

        /// <summary>
        /// Вызов события отмены
        /// </summary>
        /// <param name="args">Экземпляр <see cref="Apex.MVVM.CommandEventArgs"/> содержащего данные отмены</param>
        protected void InvokeCancelled(CommandEventArgs args)
        {
            CommandEventHandler cancelled = this.Cancelled;
            if (cancelled != null)
            {
                cancelled(this, args);
            }
        }

        /// <summary>
        /// Возникает при отмене команды
        /// </summary>
        public event CommandEventHandler Cancelled;
    }
}