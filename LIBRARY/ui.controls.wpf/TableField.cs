namespace TMP.UI.Controls.WPF
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]

    public class TableField : INotifyPropertyChanged
    {
        [IgnoreDataMember]
        [field: NonSerializedAttribute]
        public Type Type { get; set; }

        [DataMember(Name = "DeviceType")]
        public string TypeName
        {
            get => this.Type == null ? null : this.Type.AssemblyQualifiedName;
            set => this.Type = value == null ? null : Type.GetType(value);
        }

        [DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        private bool isVisible = true;

        [DataMember]
        public bool IsVisible
        {
            get => this.isVisible;

            set
            {
                if (value == this.isVisible)
                {
                    return;
                }

                this.isVisible = value;
                this.RaisePropertyChanged(nameof(this.IsVisible));
            }
        }

        public override string ToString()
        {
            return string.Format("TableField - Name '{0}', DisplayOrder '{1}', GroupName '{2}', IsVisible '{3}'",
                this.Name, this.DisplayOrder, this.GroupName, this.IsVisible);
        }

        #region INotifyPropertyChanged implementation
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged(string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
