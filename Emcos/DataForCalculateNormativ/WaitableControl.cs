using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public class WaitableControl : UserControl, INotifyPropertyChanged
    {
        private FrameworkElement _dialogHost = null;
        private string _waitMessage = string.Empty;

        public WaitableControl()
        {
            ;
        }

        public FrameworkElement DialogHost
        {
            get { return _dialogHost; }
            set { SetProperty(ref _dialogHost, value); }
        }

        public string WaitMessage
        {
            get { return _waitMessage; }
            set { SetProperty(ref _waitMessage, value); }
        }

        public void ShowWaitingScreen(string message)
        {
            App.Current.MainWindow.Cursor = Cursors.Wait;
            ProgressControl progress = new ProgressControl(message, true);
            DialogHost = progress;
        }
        public void ClearDialogHost()
        {
            DialogHost = null;
            App.Current.MainWindow.Cursor = Cursors.Arrow;
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }

    public class WaitableWindow : Window, INotifyPropertyChanged
    {
        private FrameworkElement _dialogHost = null;
        private string _waitMessage = string.Empty;

        public WaitableWindow()
        {
            ;
        }
        public FrameworkElement DialogHost
        {
            get { return _dialogHost; }
            set { SetProperty(ref _dialogHost, value); }
        }

        public string WaitMessage
        {
            get { return _waitMessage; }
            set { SetProperty(ref _waitMessage, value); }
        }

        public void ShowWaitingScreen(string message)
        {
            App.Current.MainWindow.Cursor = Cursors.Wait;
            ProgressControl progress = new ProgressControl(message, true);
            DialogHost = progress;
        }
        public void ClearDialogHost()
        {
            DialogHost = null;
            App.Current.MainWindow.Cursor = Cursors.Arrow;
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}