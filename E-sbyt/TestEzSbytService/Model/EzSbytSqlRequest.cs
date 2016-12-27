using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TMP.Work.AmperM.TestApp.Model
{
    using Shared;
    public class EzSbytRequest : INotifyPropertyChanged
    {
        private Int64 _id;
        private DateTime _date;
        private string _partFunction;
        private string _partRequest;
        private string _partParameters;

        public Int64 ID { get { return _id; } set { SetProperty(ref _id, value); } }
        public DateTime AddedDate { get { return _date; } private set { SetProperty(ref _date, value); } }
        public string PartFunction { get { return _partFunction; } set { SetProperty(ref _partFunction, value); } }
        public string PartRequest {
            get { return _partRequest; }
            set { SetProperty(ref _partRequest, value); } }
        public string PartParameters { get { return _partParameters; } set { SetProperty(ref _partParameters, value); } }

        public int Length { get { return String.IsNullOrEmpty(PartRequest) ? 0 : PartRequest.Length; } }
        public EzSbytRequest()
        {
            AddedDate = DateTime.Today;
        }
        public EzSbytRequest Clone()
        {
            return new EzSbytRequest()
            {
                AddedDate = this.AddedDate,
                PartFunction = this.PartFunction,
                PartRequest = this.PartRequest,
                PartParameters = this.PartParameters
            };
        }
        #region INotifyPropertyChanged Members

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
