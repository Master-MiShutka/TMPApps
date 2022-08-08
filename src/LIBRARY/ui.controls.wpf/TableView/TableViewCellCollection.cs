namespace TMP.UI.WPF.Controls.TableView
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class TableViewCellCollection : IList, ICollection, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
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
                    this.copyObject = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                }
            }
        }

        public TableViewCellCollection(object copyObject, int copyCount)
        {
            this.CopyCount = copyCount;
            this.CopyObject = copyObject;
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

        public int Count
        {
            get => this.CopyCount;

            set
            {
                if (value != this.CopyCount)
                {
                    this.CopyCount = value;
                    this.Reset();
                }
            }
        }

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public void Reset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

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
        #region TableCellCollection Enumerator
        private class TableCellCollectionEnumerator : IEnumerator
        {
            private int current;
            private int count;
            private object _copyObject;

            public TableCellCollectionEnumerator(TableViewCellCollection col)
            {
                this.current = -1;
                this.count = col.Count;
                this._copyObject = col.CopyObject;
            }

            public object Current => (this.current == -1) ? null : this._copyObject;

            public bool MoveNext()
            {
                if (this.current == this.count)
                {
                    return false;
                }

                ++this.current;
                return true;
            }

            public void Reset()
            {
                this.current = -1;
            }
        }
        #endregion

        public IEnumerator GetEnumerator()
        {
            return new TableCellCollectionEnumerator(this);
        }
    }
}
