using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace TMP.WORK.AramisChetchiki.Model
{
    [Serializable]
    [DataContract]
    public class SummaryInfoItem : IModel, INotifyPropertyChanged
    {
        [DataMember]
        public string FieldName { get; set; }
        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public string Info { get; set; }

        [DataMember]
        public ICollection<SummaryInfoChildItem> OnlyFirst10Items { get; set; }

        [DataMember]
        public ICollection<SummaryInfoChildItem> AllItems { get; set; }

        public SummaryInfoItem() { }
        public SummaryInfoItem(string header)
        {
            Header = header;
        }
        public SummaryInfoItem(string header, string info) : this(header)
        {
            Info = info;
        }
        [DataMember]
        public bool IsChecked { get; set; } = true;

        [IgnoreDataMember]
        private bool _ShowAllGroups = false;
        [IgnoreDataMember]
        public bool ShowAllGroups
        {
            get
            {
                return this._ShowAllGroups;
            }
            set
            {
                this._ShowAllGroups = value;
                this.RaisePropertyChanged("ShowAllGroups");
            }
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

    [Serializable]
    [DataContract]
    public class SummaryInfoChildItem : IModel
    {
        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public uint Count { get; set; }
        [DataMember]
        public double Percent { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public bool IsEmpty { get; set; }
    }
}