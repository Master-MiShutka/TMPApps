﻿namespace WpfMouseWheel.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    [Serializable]
    public class ObservableObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (this.GetType().GetProperty(propertyName) == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion

        #region Overridables

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName => false;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.VerifyPropertyName(e.PropertyName);
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            if (this.PropertyChanged != null)
            {
                foreach (var propertyName in propertyNames)
                    this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
