using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace TMP.Work.Emcos
{
    [DataContract]
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        /// <summary>
        /// Raises the PropertyChange event for the property specified
        /// </summary>
        /// <param name="propertyName">Property name to update. Is case-sensitive.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            CheckIfPropertyNameExists(propertyName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        protected bool SetProp<T>(ref T variable, T value, string name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(variable, value))
            {
                variable = value;
                RaisePropertyChanged(name);
                return true;
            }
            return false;
        }
        [Conditional("DEBUG")]
        private void CheckIfPropertyNameExists(String propertyName)
        {
            var type = this.GetType();
            Debug.Assert(
              type.GetProperty(propertyName) != null,
              propertyName + "property does not exist on object of type : " + type.FullName);
        }
    }
}
