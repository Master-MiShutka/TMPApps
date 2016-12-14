using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace TMP.Shared.Commands
{
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

        private Action _onCompleted;
        private Action _onCanceled;

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
            Initialise();
        }
        public AsynchronousDelegateCommand(Action action, Predicate<object> canExecute, string header) : base(action, canExecute, header)
        {
            // инициализация
            Initialise();
        }
        /// <summary>
        /// Создание нового экземпляра класса <see cref="AsynchronousDelegateCommand"/>
        /// </summary>
        public AsynchronousDelegateCommand(Action action, Predicate<object> canExecute, Action completed, Action canceled = null) 
            : base(action, canExecute)
        {
            _onCompleted = completed;
            _onCanceled = canceled;
            // инициализация
            Initialise();
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
            Initialise();
        }
        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, string header)
            : base(parameterizedAction, canExecute, header)
        {
            // инициализация
            Initialise();
        }
        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, Func<string> getHeader)
            : base(parameterizedAction, canExecute, getHeader)
        {
            // инициализация
            Initialise();
        }
        /// <summary>
        /// Создание нового экземпляра класса <see cref="AsynchronousDelegateCommand"/>
        /// </summary>
        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, Action completed, Action canceled = null)
            : base(parameterizedAction, canExecute)
        {
            _onCompleted = completed;
            _onCanceled = canceled;
            // инициализация
            Initialise();
        }
        public AsynchronousDelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute, Func<string> getHeader, Action completed, Action canceled = null)
            : this(parameterizedAction, canExecute, completed, canceled)
        {
            _getHeaderFunc = getHeader;
        }
        #endregion

        /// <summary>
        /// инициализация
        /// </summary>
        private void Initialise()
        {
            // создание команды отмены
            cancelCommand = new DelegateCommand(
              () =>
              {
                  //  установка флага
                  IsCancellationRequested = true;
              });
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="param">Параметр</param>
        public override void DoExecute(object param)
        {
            // Уже исполняется
            if (IsExecuting)
                return;

            // Вызов команды
            CancelCommandEventArgs args = new CancelCommandEventArgs() { Parameter = param, Cancel = false };
            InvokeExecuting(args);
            param = args.Parameter;

            // Отменена?
            if (args.Cancel)
                return;

            // Выполняется
            IsExecuting = true;

            // Сохранение диспетчера потока
            callingDispatcher = Dispatcher.CurrentDispatcher;

            // Запуск действия в новом потоке из пула
            ThreadPool.QueueUserWorkItem(
              (state) =>
              {
                  InvokeAction(param);
                  // вызов событий и установка состояния
                  ReportProgress(
                  () =>
                    {
                      // выполнение заверешено
                      IsExecuting = false;

                        // если выполнение отменено, вызов события отмены, иначе события исполнения
                        if (IsCancellationRequested)
                        {
                            InvokeCancelled(new CommandEventArgs() { Parameter = param });
                            if (_onCanceled != null) _onCanceled();
                        }
                        else
                        {
                            InvokeExecuted(new CommandEventArgs() { Parameter = param });
                            if (_onCompleted != null) _onCompleted();
                        }

                      // сброс флага
                      IsCancellationRequested = false;
                    }
              );
              }
            );
        }
        private void ReportProgress(Action action)
        {
            if (IsExecuting)
            {
                if (callingDispatcher.CheckAccess())
                    action();
                else
                    callingDispatcher.BeginInvoke(((Action)(() => { action(); })));
            }
        }
        #region Properties
        /// <summary>
        /// Отмена выполнения, если был запрос
        /// </summary>
        /// <returns>True если команда была отменена</returns>
        public bool CancelIfRequested()
        {
            if (IsCancellationRequested == false)
                return false;
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
            get
            {
                return isExecuting;
            }
            set
            {
                SetProperty(ref isExecuting, value, "IsExecuting");
            }
        }

        /// <summary>
        /// Возвращает или устанавливает значение, сообщающее запросил ли этот экземпляр отмену команды
        /// </summary>
        /// <value>
        /// <c>true</c> если этот экземпляр запросил отмену команды; иначе, <c>false</c>.
        /// </value>
        public bool IsCancellationRequested
        {
            get
            {
                return isCancellationRequested;
            }
            set
            {
                SetProperty(ref isCancellationRequested, value, "IsCancellationRequested");
            }
        }

        /// <summary>
        /// Возвращает команду отмены
        /// </summary>
        public DelegateCommand CancelCommand
        {
            get { return cancelCommand; }
        }
        #endregion


        /// <summary>
        /// Вызов события отмены
        /// </summary>
        /// <param name="args">Экземпляр <see cref="Apex.MVVM.CommandEventArgs"/> содержащего данные отмены</param>
        protected void InvokeCancelled(CommandEventArgs args)
        {
            CommandEventHandler cancelled = Cancelled;
            if (cancelled != null)
                cancelled(this, args);
        }

        /// <summary>
        /// Возникает при отмене команды
        /// </summary>
        public event CommandEventHandler Cancelled;
    }
}