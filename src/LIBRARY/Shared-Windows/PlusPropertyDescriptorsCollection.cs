namespace TMP.Shared.Windows
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using TMP.Shared.Common;

    public class PlusPropertyDescriptorsCollection : ObservableCollection<PlusPropertyDescriptor>
    {
        public PlusPropertyDescriptorsCollection()
        {

        }

        public PlusPropertyDescriptorsCollection(IEnumerable<PlusPropertyDescriptor> plusPropertyDescriptors)
        {
            IList<PlusPropertyDescriptor> items = this.Items;
            if (plusPropertyDescriptors != null && items != null)
            {
                System.Collections.IEnumerator enumerator = plusPropertyDescriptors.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    items.Add((PlusPropertyDescriptor)enumerator.Current);
                }
            }
        }

        public void RemoveRange(int index, int count)
        {
            this.CheckReentrancy();
            var items = this.Items as List<PlusPropertyDescriptor>;
            items.RemoveRange(index, count);
            this.OnReset();
        }

        public void InsertRange(int index, IEnumerable<PlusPropertyDescriptor> collection)
        {
            this.CheckReentrancy();
            var items = this.Items as List<PlusPropertyDescriptor>;
            items.InsertRange(index, collection);
            this.OnReset();
        }

        private void OnReset()
        {
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
