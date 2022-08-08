using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TMP.ARMTES
{
    /// <summary>
    /// Интерфейс <see cref="IDataView" />
    /// </summary>
    public interface IDataView
    {
        string Header { get; set; }
        IEnumerable<object> Items { get; set; }
        ICollection<ColumnDescriptor> Columns { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="DataItem" />
    /// </summary>
    public class DataItem : IDataView
    {
        /// <summary>
        /// Gets or sets the Header
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the Items
        /// </summary>
        public IEnumerable<object> Items { get; set; }

        /// <summary>
        /// Gets or sets the Columns
        /// </summary>
        public ICollection<ColumnDescriptor> Columns { get; set; }
    }

    /// <summary>
    /// Список <see cref="DataViewList" />
    /// </summary>
    public class DataViewList : IList<IDataView>, INotifyCollectionChanged
    {
        private List<IDataView> _list = new List<IDataView>();
        private bool _isRaisingEvent;

        /// <summary>
        /// Defines the CollectionChanged
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="item">The <see cref="IDataView"/></param>
        public void Add(IDataView item)
        {
            ThrowOnReentrancy();
            ThrowIfValueIsNull(item);
            _list.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _list.Count - 1));
        }

        /// <summary>
        /// The AddRange
        /// </summary>
        /// <param name="items">The <see cref="IEnumerable{IDataView}"/></param>
        public void AddRange(IEnumerable<IDataView> items)
        {
            InsertRange(this.Count, items);
        }

        /// <summary>
        /// The Clear
        /// </summary>
        public void Clear()
        {
            ThrowOnReentrancy();
            var oldList = _list;
            _list = new List<IDataView>();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldList, 0));
        }

        /// <summary>
        /// The Contains
        /// </summary>
        /// <param name="item">The <see cref="IDataView"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Contains(IDataView item)
        {
            return IndexOf(item) >= 0;
        }

        public IDataView this[int index]
        {
            get => _list[index];
            set
            {
                ThrowOnReentrancy();
                var oldItem = _list[index];
                if (oldItem == value)
                    return;
                _list[index] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }
        /// <summary>
        /// Gets the Count
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Gets a value indicating whether IsReadOnly
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// The IndexOf
        /// </summary>
        /// <param name="item">The <see cref="IDataView"/></param>
        /// <returns>The <see cref="int"/></returns>
        public int IndexOf(IDataView item)
        {
            if (item == null)
                return -1;
            else
                return _list.IndexOf(item);
        }

        /// <summary>
        /// The Insert
        /// </summary>
        /// <param name="index">The <see cref="int"/></param>
        /// <param name="item">The <see cref="IDataView"/></param>
        public void Insert(int index, IDataView item)
        {
            ThrowOnReentrancy();
            ThrowIfValueIsNull(item);
            _list.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// The InsertRange
        /// </summary>
        /// <param name="index">The <see cref="int"/></param>
        /// <param name="items">The <see cref="IEnumerable{IDataView}"/></param>
        public void InsertRange(int index, IEnumerable<IDataView> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            ThrowOnReentrancy();
            List<IDataView> newItems = items.ToList();
            if (newItems.Count == 0)
                return;
            foreach (IDataView item in newItems)
            {
                ThrowIfValueIsNull(item);
            }
            _list.InsertRange(index, newItems);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, index));
        }

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="item">The <see cref="IDataView"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Remove(IDataView item)
        {
            int pos = IndexOf(item);
            if (pos >= 0)
            {
                RemoveAt(pos);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The RemoveAt
        /// </summary>
        /// <param name="index">The <see cref="int"/></param>
        public void RemoveAt(int index)
        {
            ThrowOnReentrancy();
            var oldItem = _list[index];
            _list.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
        }

        /// <summary>
        /// The RemoveRange
        /// </summary>
        /// <param name="index">The <see cref="int"/></param>
        /// <param name="count">The <see cref="int"/></param>
        public void RemoveRange(int index, int count)
        {
            ThrowOnReentrancy();
            if (count == 0)
                return;
            var oldItems = _list.GetRange(index, count);
            _list.RemoveRange(index, count);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, index));
        }

        /// <summary>
        /// The RemoveAll
        /// </summary>
        /// <param name="match">The <see cref="Predicate{IDataView}"/></param>
        public void RemoveAll(Predicate<IDataView> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            ThrowOnReentrancy();
            int firstToRemove = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                bool removeItem;
                _isRaisingEvent = true;
                try
                {
                    removeItem = match(_list[i]);
                }
                finally
                {
                    _isRaisingEvent = false;
                }
                if (!removeItem)
                {
                    if (firstToRemove < i)
                    {
                        RemoveRange(firstToRemove, i - firstToRemove);
                        i = firstToRemove - 1;
                    }
                    else
                    {
                        firstToRemove = i + 1;
                    }
                    System.Diagnostics.Debug.Assert(firstToRemove == i + 1);
                }
            }
            if (firstToRemove < _list.Count)
            {
                RemoveRange(firstToRemove, _list.Count - firstToRemove);
            }
        }

        /// <summary>
        /// The CopyTo
        /// </summary>
        /// <param name="array">The <see cref="IDataView[]"/></param>
        /// <param name="arrayIndex">The <see cref="int"/></param>
        public void CopyTo(IDataView[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="IEnumerator{IDataView}"/></returns>
        public IEnumerator<IDataView> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="System.Collections.IEnumerator"/></returns>
        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// The OnCollectionChanged
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.Assert(!_isRaisingEvent);
            _isRaisingEvent = true;
            try
            {
                if (CollectionChanged != null)
                    CollectionChanged(this, e);
            }
            finally
            {
                _isRaisingEvent = false;
            }
        }

        /// <summary>
        /// The ThrowOnReentrancy
        /// </summary>
        private void ThrowOnReentrancy()
        {
            if (_isRaisingEvent)
                throw new InvalidOperationException();
        }

        /// <summary>
        /// The ThrowIfValueIsNull
        /// </summary>
        /// <param name="item">The <see cref="IDataView"/></param>
        private void ThrowIfValueIsNull(IDataView item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
        }
    }
}
