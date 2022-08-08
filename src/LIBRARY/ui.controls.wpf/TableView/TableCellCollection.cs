namespace TMP.UI.WPF.Controls.TableView
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;

    public class TableCellCollection : IList, ICollection, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public int CopyCount { get; set; }

        private object copyObject;

        public object CopyObject
        {
            get => this.copyObject;

            set
            {
                if (value != this.copyObject)
                {
                    // var oldItem = _copyObject;
                    this.copyObject = value;

                    this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

                    // for (int i = 0; i < Count; ++i)
                    //  OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, i));
                }
            }
        }

        public TableCellCollection(object copyObject, int copyCount)
        {
            this.CopyObject = copyObject;
            this.CopyCount = copyCount;
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            return value == this.CopyObject;
        }

        public int IndexOf(object value)
        {
            if (value == this.CopyObject)
            {
                return 0;
            }

            return -1;
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => true;

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get => this.CopyObject;
            set => throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count => this.CopyCount;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, e);
            }
        }

        //-------------------------------------------------------
        private class TableCellCollectionEnumerator : IEnumerator
        {
            private int current;
            private int _count;
            private object copyObject;

            public TableCellCollectionEnumerator(TableCellCollection col)
            {
                this.current = -1;
                this._count = col.Count;
                this.copyObject = col.CopyObject;
            }

            public object Current => (this.current == -1) ? null : this.copyObject;

            public bool MoveNext()
            {
                if (this.current == this._count)
                {
                    return false;
                }

                this.current++;
                return true;
            }

            public void Reset()
            {
                this.current = -1;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new TableCellCollectionEnumerator(this);
        }
    }
}
