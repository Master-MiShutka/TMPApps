using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Reflection;

namespace TMP.Work.Emcos
{
    public class EmcosSettings : INotifyPropertyChanged
    {
        private string _userName, _password, _serverAddress;
        private int _netTimeOutInSeconds;

        public EmcosSettings()
        {
            ;
        }
        public EmcosSettings(ApplicationSettingsBase settings)
        {
            if (settings == null)
                throw new ArgumentNullException();
            settings.SettingChanging += Settings_SettingChanging;
        }

        private void Settings_SettingChanging(object sender, SettingChangingEventArgs e)
        {
            if (e.SettingName == "UserName")
                UserName = e.NewValue.ToString();
            if (e.SettingName == "NetTimeOutInSeconds")
                NetTimeOutInSeconds = (int)e.NewValue;
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                SetProperty(ref _userName, value, "UserName");
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                SetProperty(ref _password, value, "Password");
            }
        }
        public string ServerAddress
        {
            get { return _serverAddress; }
            set
            {
                _serverAddress = value;
                SetProperty(ref _serverAddress, value, "ServerAddress");
            }
        }
        public int NetTimeOutInSeconds
        {
            get { return _netTimeOutInSeconds; }
            set
            {
                _netTimeOutInSeconds = value;
                SetProperty(ref _netTimeOutInSeconds, value, "NetTimeOutInSeconds");
            }
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides
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
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;
        public bool SetProperty<T>(ref T storage, T value, string propertyName = null)
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
        #endregion // INotifyPropertyChanged Members
    }
}