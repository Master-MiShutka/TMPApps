using System;
using System.ComponentModel;

namespace TMP.ARMTES.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private string _status = null;
        public String Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged("Status"); }
        }
        private string _detailedStatus = null;
        public String DetailedStatus
        {
            get { return _detailedStatus; }
            set { _detailedStatus = value; RaisePropertyChanged("DetailedStatus"); }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged("IsBusy"); }
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
}
