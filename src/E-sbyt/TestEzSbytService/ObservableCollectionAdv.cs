using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace TMP.Work.AmperM.TestApp
{
    public class ObservableCollectionAdv<T> : ObservableCollection<T>
    {
        private object _selectedItem;
        public ObservableCollectionAdv()
        {
            this.CollectionChanged += ObservableCollectionAdv_CollectionChanged;
        }

        private void ObservableCollectionAdv_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                SelectedItem = e.NewItems[0];
            }
        }

        public object SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged("SelectedItem"); }
        }

        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
