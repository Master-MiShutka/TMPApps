using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TMP.Shared.Commands
{
    public class DelegateCommand : ICommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Действие(или параметризованное действие) которое вызывается при активации команды.
        /// </summary>
        protected Action _action = null;
        protected Action<object> _parameterizedAction = null;
        /// <summary>
        /// Предикат, определяющий возможность выполнения команды.
        /// </summary>
        protected readonly Predicate<object> _canExecute;

        protected Func<string> _getHeaderFunc = null;
        protected string _header = null;
        public string Header
        {
            get { return (_getHeaderFunc == null) ? (_header == null ? "<без названия>" : _header) : _getHeaderFunc(); }
            set { if (_getHeaderFunc == null) SetProperty(ref _header, value, "Header"); }
        }

        public DelegateCommand(Action action)
        {
            _action = action;
        }
        public DelegateCommand(Action<object> action)
        {
            _parameterizedAction = action;
        }
        public DelegateCommand(Action action, string header) : this(action)
        {
            _header = header;
        }
        public DelegateCommand(Action<object> action, string header) : this(action)
        {
            _header = header;
        }
        public DelegateCommand(Action action, Predicate<object> canExecute) : this(action)
        {
            _canExecute = canExecute;
        }
        public DelegateCommand(Action<object> action, Predicate<object> canExecute) : this(action)
        {
            _canExecute = canExecute;
        }
        public DelegateCommand(Action action, Predicate<object> canExecute, string header) : this(action, canExecute)
        {
            _header = header;
        }
        public DelegateCommand(Action<object> action, Predicate<object> canExecute, string header) : this(action, canExecute)
        {
            _header = header;
        }

        public DelegateCommand(Action action, Predicate<object> canExecute, Func<string> getHeader) : this(action, canExecute)
        {
            _getHeaderFunc = getHeader;
        }
        public DelegateCommand(Action<object> action, Predicate<object> canExecute, Func<string> getHeader) : this(action, canExecute)
        {
            _getHeaderFunc = getHeader;
        }
        #region Выполнение
        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="param">Параметр</param>
        public virtual void DoExecute(object param)
        {
            // Вызов выполнения команды с возможностью отмены
            CancelCommandEventArgs args =
               new CancelCommandEventArgs() { Parameter = param, Cancel = false };
            InvokeExecuting(args);

            //  Если событие было отменено - остановка
            if (args.Cancel)
                return;

            // Вызов действия
            InvokeAction(param);

            // Сообщение о выполнении команды
            InvokeExecuted(new CommandEventArgs() { Parameter = param });
        }
        #endregion
        #region ICommand implementation

        /// <summary>
        /// Определяем метод, определющий, что выполнение команды допускается в текущем состоянии
        /// </summary>
        /// <param name="parameter">Этот параметр используется командой.
        ///  Если команда вызывается без использования параметра,
        ///  то этот объект может быть установлен в  null.</param>
        /// <returns>
        /// если выполнение команды разрешено - true, если запрещено - false.
        /// </returns>
        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="param">Параметр</param>
        void ICommand.Execute(object parameter)
        {
            this.DoExecute(parameter);

        }
        /// <summary>
        /// Вызывается, когда меняется возможность выполнения команды
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }
        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
        #endregion
        #region События

        /// <summary>
        /// Вызывается во время выполнения команды
        /// </summary>
        public event CancelCommandEventHandler Executing;
        /// <summary>
        /// Вызывается, когда команда выполнена
        /// </summary>
        public event CommandEventHandler Executed;

        #endregion

        #region Invokes
        protected void InvokeAction(object param)
        {
            Action theAction = _action;
            Action<object> theParameterizedAction = _parameterizedAction;
            if (theAction != null)
                theAction();
            else if (theParameterizedAction != null)
                theParameterizedAction(param);
        }

        protected void InvokeExecuted(CommandEventArgs args)
        {
            CommandEventHandler executed = Executed;
            if (executed != null)
                executed(this, args);
        }

        protected void InvokeExecuting(CancelCommandEventArgs args)
        {
            CancelCommandEventHandler executing = Executing;
            if (executing != null)
                executing(this, args);
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// The CommandEventHandler delegate.
    /// </summary>
    public delegate void CommandEventHandler(object sender, CommandEventArgs args);

    /// <summary>
    /// The CancelCommandEvent delegate.
    /// </summary>
    public delegate void CancelCommandEventHandler(object sender, CancelCommandEventArgs args);

    /// <summary>
    /// CommandEventArgs - simply holds the command parameter.
    /// </summary>
    public class CommandEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the parameter.
        /// </summary>
        /// <value>The parameter.</value>
        public object Parameter { get; set; }
    }

    /// <summary>
    /// CancelCommandEventArgs - just like above but allows the event to 
    /// be cancelled.
    /// </summary>
    public class CancelCommandEventArgs : CommandEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CancelCommandEventArgs"/> command should be cancelled.
        /// </summary>
        /// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
        public bool Cancel { get; set; }
    }
}
