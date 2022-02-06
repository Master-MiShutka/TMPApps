namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [KnownType(typeof(SummaryInfoChildItem))]
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class SummaryInfoItem : IModel, INotifyPropertyChanged
    {
        [MessagePack.IgnoreMember]
        private string fieldName;
        [MessagePack.IgnoreMember]
        private string header;
        [MessagePack.IgnoreMember]
        private string info;
        [MessagePack.IgnoreMember]
        private ICollection<SummaryInfoChildItem> onlyFirst10Items;
        [MessagePack.IgnoreMember]
        private ICollection<SummaryInfoChildItem> allItems;
        [MessagePack.IgnoreMember]
        private bool isChecked = true;
        [MessagePack.IgnoreMember]
        private bool showAllGroups = false;

        public SummaryInfoItem()
        {
        }

        public SummaryInfoItem(string header)
        {
            this.header = header;
        }

        public SummaryInfoItem(string header, string info)
            : this(header)
        {
            this.info = info;
        }

        public SummaryInfoItem(string fieldName, string header, string info)
            : this(header, info)
        {
            this.fieldName = fieldName;
        }

        public string FieldName
        {
            get => this.fieldName;
            set => this.SetProperty(ref this.fieldName, value);
        }

        public string Header
        {
            get => this.header;
            set => this.SetProperty(ref this.header, value);
        }

        public string Info
        {
            get => this.info;
            set => this.SetProperty(ref this.info, value);
        }

        public ICollection<SummaryInfoChildItem> OnlyFirst10Items
        {
            get => this.onlyFirst10Items;
            set => this.SetProperty(ref this.onlyFirst10Items, value);
        }

        public ICollection<SummaryInfoChildItem> AllItems
        {
            get => this.allItems;
            set => this.SetProperty(ref this.allItems, value);
        }

        public bool IsChecked
        {
            get => this.isChecked;
            set => this.SetProperty(ref this.isChecked, value);
        }

        [MessagePack.IgnoreMember]
        public bool ShowAllGroups
        {
            get => this.showAllGroups;
            set => this.SetProperty(ref this.showAllGroups, value);
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (propertyName != null && TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                {
                    throw new System.Exception(msg);
                }
                else
                {
                    System.Diagnostics.Debug.Fail(msg);
                }
            }
        }

        [System.Runtime.Serialization.IgnoreDataMember]
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(storage, value))
            {
                storage = value;
                this.RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.VerifyPropertyName(propertyName);

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }

    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class SummaryInfoChildItem : IModel, INotifyPropertyChanged
    {
        [MessagePack.IgnoreMember]
        private string header;
        [MessagePack.IgnoreMember]
        private uint count;
        [MessagePack.IgnoreMember]
        private double percent;
        [MessagePack.IgnoreMember]
        private string value;
        [MessagePack.IgnoreMember]
        private bool isEmpty;

        public string Header
        {
            get => this.header;
            set => this.SetProperty(ref this.header, value);
        }

        public uint Count
        {
            get => this.count;
            set => this.SetProperty(ref this.count, value);
        }

        public double Percent
        {
            get => this.percent;
            set => this.SetProperty(ref this.percent, value);
        }

        public string Value
        {
            get => this.value;
            set => this.SetProperty(ref this.value, value);
        }

        public bool IsEmpty
        {
            get => this.isEmpty;
            set => this.SetProperty(ref this.isEmpty, value);
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (propertyName != null && TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                {
                    throw new System.Exception(msg);
                }
                else
                {
                    System.Diagnostics.Debug.Fail(msg);
                }
            }
        }

        [System.Runtime.Serialization.IgnoreDataMember]
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(storage, value))
            {
                storage = value;
                this.RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.VerifyPropertyName(propertyName);

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}