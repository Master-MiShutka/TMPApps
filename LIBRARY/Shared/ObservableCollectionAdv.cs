namespace TMP.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;

    public class ObservableCollectionAdv<T> : ObservableCollection<T>
    {
        public void RemoveRange(int index, int count)
        {
            this.CheckReentrancy();
            var items = this.Items as List<T>;
            items.RemoveRange(index, count);
            this.OnReset();
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            this.CheckReentrancy();
            var items = this.Items as List<T>;
            items.InsertRange(index, collection);
            this.OnReset();
        }

        private void OnReset()
        {
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
