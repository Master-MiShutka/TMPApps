using System;
using System.Windows.Input;

namespace TMP.Wpf.Common
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public string Header { get; set; }

        public DelegateCommand(Action action)
        {
            _action = action;
            Header = "<без названия>";
        }
        public DelegateCommand(Action action, string header)
        {
            _action = action;
            Header = header;
        }
        public DelegateCommand(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
            Header = "<без названия>";
        }
        public DelegateCommand(Action action, Func<bool> canExecute, string header) : this(action, canExecute)
        {
            Header = header;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

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

    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _action;
        private readonly Func<bool> _canExecute;
        public string Header { get; set; }
        public DelegateCommand(Action<T> action)
        {
            _action = action;
            Header = "<без названия>";
        }
        public DelegateCommand(Action<T> action, string header)
        {
            _action = action;
            Header = header;
        }
        public DelegateCommand(Action<T> action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
            Header = "<без названия>";
        }

        public void Execute(object parameter)
        {
            _action((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

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
    }
}
