namespace TMP.Shared.Commands
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class DelegateCommand : ICommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Действие(или параметризованное действие) которое вызывается при активации команды.
        /// </summary>
        protected Action action = null;
        protected Action<object> parameterizedAction = null;

        /// <summary>
        /// Предикат, определяющий возможность выполнения команды.
        /// </summary>
        protected readonly Predicate<object> canExecute;

        protected Func<string> getHeaderFunc = null;
        protected string header;
        protected string toolTip;
        private object tag;

        public string Header
        {
            get => (this.getHeaderFunc == null) ? (this.header ?? "<без названия>") : this.getHeaderFunc();

            set
            {
                if (this.getHeaderFunc == null)
                {
                    this.SetProperty(ref this.header, value, nameof(this.Header));
                }
            }
        }

        public string ToolTip
        {
            get => this.toolTip;
            set => this.SetProperty(ref this.toolTip, value, nameof(this.ToolTip));
        }

        public object Tag
        {
            get => this.tag;
            set => this.SetProperty(ref this.tag, value, nameof(this.Tag));
        }

        public DelegateCommand(Action action)
        {
            this.action = action;
        }

        public DelegateCommand(Action action, string header, object tag = null) : this(action)
        {
            this.header = header;
            this.tag = tag;
        }

        public DelegateCommand(Action action, string header, string tooltip, object tag = null) : this(action, header, tag)
        {
            this.toolTip = tooltip;
        }

        public DelegateCommand(Action<object> parameterizedAction)
        {
            this.parameterizedAction = parameterizedAction;
        }

        public DelegateCommand(Action<object> action, string header, object tag = null) : this(action)
        {
            this.header = header;
            this.tag = tag;
        }

        public DelegateCommand(Action<object> action, string header, string tooltip, object tag = null) : this(action, header, tag)
        {
            this.toolTip = tooltip;
        }

        public DelegateCommand(Action action, Predicate<object> canExecute) : this(action)
        {
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action<object> action, Predicate<object> canExecute) : this(action)
        {
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action action, Predicate<object> canExecute, string header) : this(action, canExecute)
        {
            this.header = header;
        }

        public DelegateCommand(Action<object> action, Predicate<object> canExecute, string header) : this(action, canExecute)
        {
            this.header = header;
        }

        public DelegateCommand(Action action, Predicate<object> canExecute, Func<string> getHeader) : this(action, canExecute)
        {
            this.getHeaderFunc = getHeader;
        }

        public DelegateCommand(Action<object> action, Predicate<object> canExecute, Func<string> getHeader) : this(action, canExecute)
        {
            this.getHeaderFunc = getHeader;
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
            this.InvokeExecuting(args);

            // Если событие было отменено - остановка
            if (args.Cancel)
            {
                return;
            }

            // Вызов действия
            this.InvokeAction(param);

            // Сообщение о выполнении команды
            this.InvokeExecuted(new CommandEventArgs() { Parameter = param });
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
            return this.canExecute == null ? true : this.canExecute(parameter);
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
            Action theAction = this.action;
            Action<object> theParameterizedAction = this.parameterizedAction;
            if (theAction != null)
            {
                theAction();
            }
            else
            {
                theParameterizedAction?.Invoke(param);
            }
        }

        protected void InvokeExecuted(CommandEventArgs args)
        {
            this.Executed?.Invoke(this, args);
        }

        protected void InvokeExecuting(CancelCommandEventArgs args)
        {
            this.Executing?.Invoke(this, args);
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

    public class DelegateCommand<T> : ICommand, INotifyPropertyChanged
    {
        private readonly Action<T> action;
        private readonly Predicate<object> canExecute;

        protected Func<string> _getHeaderFunc = null;
        protected string _header;
        protected string _toolTip;
        private object _tag;

        public string Header
        {
            get => (this._getHeaderFunc == null) ? (this._header ?? "<без названия>") : this._getHeaderFunc();

            set
            {
                if (this._getHeaderFunc == null)
                {
                    this.SetProperty(ref this._header, value, nameof(this.Header));
                }
            }
        }

        public string ToolTip
        {
            get => this._toolTip;
            set => this.SetProperty(ref this._toolTip, value, nameof(this.ToolTip));
        }

        public object Tag
        {
            get => this._tag;
            set => this.SetProperty(ref this._tag, value, nameof(this.Tag));
        }

        public DelegateCommand(Action<T> action)
        {
            this.action = action;
            this.Header = "<без названия>";
        }

        public DelegateCommand(Action<T> action, string header) : this(action)
        {
            this.Header = header;
        }

        public DelegateCommand(Action<T> action, string header, object tag = null) : this(action, header)
        {
            this._tag = tag;
        }

        public DelegateCommand(Action<T> action, string header, string tooltip, object tag = null) : this(action, header, tag)
        {
            this._toolTip = tooltip;
        }

        public DelegateCommand(Action<T> action, Predicate<object> canExecute) : this(action)
        {
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action<T> action, Predicate<object> canExecute, string header) : this(action, canExecute)
        {
            this._header = header;
        }

        public DelegateCommand(Action<T> action, Predicate<object> canExecute, string header, string tooltip) : this(action, canExecute, header)
        {
            this._toolTip = tooltip;
        }

        public DelegateCommand(Action<T> action, Predicate<object> canExecute, string header, string tooltip, object tag = null) : this(action, canExecute, header, tooltip)
        {
            this._tag = tag;
        }

        public DelegateCommand(Action<T> action, Predicate<object> canExecute, Func<string> getHeader) : this(action, canExecute)
        {
            this._getHeaderFunc = getHeader;
        }

        #region Выполнение

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="param">Параметр</param>
        public virtual void DoExecute(T param)
        {
            // Вызов выполнения команды с возможностью отмены
            CancelCommandEventArgs args =
               new CancelCommandEventArgs() { Parameter = param, Cancel = false };
            this.InvokeExecuting(args);

            // Если событие было отменено - остановка
            if (args.Cancel)
            {
                return;
            }

            // Вызов действия
            this.InvokeAction(param);

            // Сообщение о выполнении команды
            this.InvokeExecuted(new CommandEventArgs() { Parameter = param });
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
            return this.canExecute == null ? true : this.canExecute(parameter);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="param">Параметр</param>
        void ICommand.Execute(object parameter)
        {
            this.DoExecute((T)parameter);
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
        protected void InvokeAction(T param)
        {
            this.action?.Invoke(param);
        }

        protected void InvokeExecuted(CommandEventArgs args)
        {
            this.Executed?.Invoke(this, args);
        }

        protected void InvokeExecuting(CancelCommandEventArgs args)
        {
            this.Executing?.Invoke(this, args);
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<P>(ref P storage, P value, string propertyName)
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
}
