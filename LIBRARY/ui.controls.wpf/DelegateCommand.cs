using System;
using System.ComponentModel;
using System.Windows.Input;

namespace TMP.UI.Controls.WPF
{
    public class DelegateCommand : ICommand, INotifyPropertyChanged
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        private string _header, _toolTip;
        private object _tag;

        public string Header
        {
            get { return _header; }
            set { _header = value; RaisePropertyChanged("Header"); }
        }
        public string ToolTip
        {
            get { return _toolTip; }
            set { _toolTip = value; RaisePropertyChanged("ToolTip"); }
        }
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; RaisePropertyChanged("Tag"); }
        }

        public DelegateCommand(Action action)
        {
            _action = action;
            Header = "<без названия>";
        }
        public DelegateCommand(Action action, string header, object tag = null)
        {
            _action = action;
            Header = header;
            _tag = tag;
        }
        public DelegateCommand(Action action, string header, string tooltip, object tag = null)
        {
            _action = action;
            Header = header;
            ToolTip = tooltip;
            _tag = tag;
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

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion

    }

    public class DelegateCommand<T> : ICommand, INotifyPropertyChanged
    {
        private readonly Action<T> _action;
        private readonly Func<bool> _canExecute;
        private string _header, _toolTip;

        public string Header
        {
            get { return _header; }
            set { _header = value; RaisePropertyChanged("Header"); }
        }
        public string ToolTip
        {
            get { return _toolTip; }
            set { _toolTip = value; RaisePropertyChanged("ToolTip"); }
        }
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
        public DelegateCommand(Action<T> action, string header, string tooltip)
        {
            _action = action;
            Header = header;
            ToolTip = tooltip;
        }
        public DelegateCommand(Action<T> action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
            Header = "<без названия>";
        }
        public DelegateCommand(Action<T> action, Func<bool> canExecute, string header) : this(action, canExecute)
        {
            Header = header;
        }
        public DelegateCommand(Action<T> action, Func<bool> canExecute, string header, string tooltip) : this(action, canExecute, header)
        {
            ToolTip = tooltip;
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

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
