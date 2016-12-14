using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace TMP.Work.Emcos
{
    class MagicAttribute : Attribute { }

    /// <summary>
    /// http://habrahabr.ru/post/95211/
    /// </summary>

    [Magic]
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
#if DEBUG
            App.LOG.Log(String.Format(
                "{0}\t{1}\t{2}",
                "PropertyChangedBase",
                "RaisePropertyChanged",
                propertyName), Common.Logger.Category.Info, Common.Logger.Priority.None);
#endif
            checkIfPropertyNameExists(propertyName);

            var e = PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        protected bool SetProp<T>(ref T variable, T value, string name)
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
        private void checkIfPropertyNameExists(String propertyName)
        {
            var type = this.GetType();
            Debug.Assert(
              type.GetProperty(propertyName) != null,
              propertyName + "property does not exist on object of type : " + type.FullName);
        }
    }
}
