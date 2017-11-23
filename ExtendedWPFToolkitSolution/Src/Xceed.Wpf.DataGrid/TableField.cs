using System;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
    [Serializable]
    public class TableField : INotifyPropertyChanged
    {
        public Type Type { get; set; }
        public int DisplayOrder { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { if (value == _isVisible) return; _isVisible = value; RaisePropertyChanged("IsVisible"); }
        }

        public override string ToString()
        {
            return String.Format("TableField - Name '{0}', DisplayOrder '{1}', GroupName '{2}', IsVisible '{3}'",
                Name, DisplayOrder, GroupName, IsVisible);
        }

        #region INotifyPropertyChanged implementation
        [field: NonSerializedAttribute()]
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
