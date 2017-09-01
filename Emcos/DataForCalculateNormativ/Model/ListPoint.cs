using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using TMP.Work.Emcos.Model;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    [Serializable]
    [DataContract]
    public class ListPoint : INotifyPropertyChanged
    {
        [IgnoreDataMember]
        private bool _isChecked = false;

        [DataMember]
        public int ParentId { get; set; }
        [DataMember]
        public string ParentTypeCode { get; set; }
        [DataMember]
        public string ParentName { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsGroup { get; set; }
        [DataMember]
        public string TypeCode { get; set; }
        [DataMember]
        public string EсpName { get; set; }
        [DataMember]
        public ElementTypes Type { get; set; }
        [DataMember]
        public IList<ListPoint> Items { get; set; }
        [DataMember]
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

        public ListPoint()
        {
            Items = new List<ListPoint>();
        }
        public override string ToString()
        {
            return string.Format("Id:{0}, Name:{1}, TypeCode:{2}",
                Id,
                Name,
                TypeCode);
        }
        public override bool Equals(object obj)
        {
            ListPoint o = obj as ListPoint;
            if (o == null) return false;

            return this.Id == o.Id && this.Name == o.Name;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #region INotifyPropertyChanged Members

        #region Debugging Aides
        [IgnoreDataMember]
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
        [field: NonSerializedAttribute()]
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