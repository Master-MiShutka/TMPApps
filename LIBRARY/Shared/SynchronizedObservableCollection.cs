namespace TMP.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable]
    [ComVisible(false)]
    [DebuggerDisplay("Count = {Count}")]
    public class SynchronizedObservableCollection<T> : IDisposable, IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private bool isDisposed;
        private readonly List<T> items = new List<T>();

        [NonSerialized]
        private readonly ReaderWriterLockSlim itemsLocker = new ReaderWriterLockSlim();
        [NonSerialized]
        private SynchronizationContext context = SynchronizationContext.Current ?? new SynchronizationContext();
        private readonly SimpleMonitor monitor = new ();
        [NonSerialized]
        private object syncRoot;

        private static bool IsCompatibleObject(object value) =>
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            (value is T) || (value == null && default(T) == null);

        public SynchronizedObservableCollection()
            : this(System.Linq.Enumerable.Empty<T>())
        {
        }

        public SynchronizedObservableCollection(SynchronizationContext context)
            : this(System.Linq.Enumerable.Empty<T>())
        {
            this.context = context;
        }

        public SynchronizedObservableCollection(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                this.Add(item);
            }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this.PropertyChanged += value; }
            remove { this.PropertyChanged -= value; }
        }

        /// <summary>Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Occurs when a property value changes.</summary>
        protected event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Gets a value indicating whether the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <returns>true if the <see cref="SynchronizedObservableCollection{T}" /> has a fixed size; otherwise, false.</returns>
        protected bool IsFixedSize => false;

        bool IList.IsFixedSize => this.IsFixedSize;

        /// <summary>Gets a value indicating whether the <see cref="SynchronizedObservableCollection{T}" /> is read-only.</summary>
        /// <returns>true if the <see cref="SynchronizedObservableCollection{T}" /> is read-only; otherwise, false.</returns>
        protected bool IsReadOnly => false;

        bool ICollection<T>.IsReadOnly => this.IsReadOnly;

        bool IList.IsReadOnly => this.IsReadOnly;

        /// <summary>Gets a value indicating whether access to the <see cref="SynchronizedObservableCollection{T}" /> is synchronized (thread safe).</summary>
        /// <returns>true if access to the <see cref="SynchronizedObservableCollection{T}" /> is synchronized (thread safe); otherwise, false.</returns>
        protected bool IsSynchronized => true;

        bool ICollection.IsSynchronized => this.IsSynchronized;

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="SynchronizedObservableCollection{T}" />.</returns>
        protected object SyncRoot
        {
            get
            {
                // ReSharper disable once InvertIf
                if (this.syncRoot == null)
                {
                    this.itemsLocker.EnterReadLock();

                    try
                    {
                        if (this.items is ICollection collection)
                        {
                            this.syncRoot = collection.SyncRoot;
                        }
                        else
                        {
                            Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), null);
                        }
                    }
                    finally
                    {
                        this.itemsLocker.ExitReadLock();
                    }
                }

                return this.syncRoot;
            }
        }

        object ICollection.SyncRoot => this.SyncRoot;

        object IList.this[int index]
        {
            get => this[index];

            set
            {
                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("'value' is the wrong type");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="SynchronizationContext"/> that events will be invoked on.
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public SynchronizationContext Context => this.context;

        /// <summary>Gets the number of elements actually contained in the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <returns>The number of elements actually contained in the <see cref="SynchronizedObservableCollection{T}" />.</returns>
        public int Count
        {
            get
            {
                this.itemsLocker.EnterReadLock();

                try
                {
                    return this.items.Count;
                }
                finally
                {
                    this.itemsLocker.ExitReadLock();
                }
            }
        }

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="SynchronizedObservableCollection{T}.Count" />. </exception>
        public T this[int index]
        {
            get
            {
                this.itemsLocker.EnterReadLock();

                try
                {
                    this.CheckIndex(index);

                    return this.items[index];
                }
                finally
                {
                    this.itemsLocker.ExitReadLock();
                }
            }

            set
            {
                this.Set(value, index);
            }
        }

        protected virtual void Set(T item, int index)
        {
            T oldValue;

            this.itemsLocker.EnterWriteLock();

            try
            {
                this.CheckIndex(index);
                this.CheckReentrancy();

                oldValue = this.items[index];

                this.items[index] = item;
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyItemReplaced(item, oldValue, index);
        }

        private IDisposable BlockReentrancy()
        {
            this.monitor.Enter();

            return this.monitor;
        }

        // ReSharper disable once UnusedParameter.Local
        private void CheckIndex(int index)
        {
            if (index < 0 || index >= this.items.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckReentrancy()
        {
            if (this.monitor.Busy && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1)
            {
                throw new InvalidOperationException("SynchronizedObservableCollection reentrancy not allowed");
            }
        }

        private void OnNotifyCollectionReset()
        {
            using (this.BlockReentrancy())
            {
                this.context.Send(state =>
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }, null);
            }
        }

        private void OnNotifyItemAdded(T item, int index)
        {
            using (this.BlockReentrancy())
            {
                this.context.Send(state =>
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                }, null);
            }
        }

        private void OnNotifyItemMoved(T item, int newIndex, int oldIndex)
        {
            using (this.BlockReentrancy())
            {
                this.context.Send(state =>
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
                }, null);
            }
        }

        private void OnNotifyItemRemoved(T item, int index)
        {
            using (this.BlockReentrancy())
            {
                this.context.Send(state =>
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                }, null);
            }
        }

        private void OnNotifyItemReplaced(T newItem, T oldItem, int index)
        {
            using (this.BlockReentrancy())
            {
                this.context.Send(state =>
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
                }, null);
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="SynchronizedObservableCollection{T}"/>.
        /// </summary>
        /// <param name="disposing">Not used.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.itemsLocker.Dispose();
            this.isDisposed = true;
        }

        /// <summary>Adds an object to the end of the <see cref="SynchronizedObservableCollection{T}" />. </summary>
        /// <param name="item">The object to be added to the end of the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
        public void Add(T item)
        {
            this.itemsLocker.EnterWriteLock();

            int index;

            try
            {
                this.CheckReentrancy();

                index = this.items.Count;

                this.items.Insert(index, item);
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyItemAdded(item, index);
        }

        int IList.Add(object value)
        {
            this.itemsLocker.EnterWriteLock();

            int index;
            T item;

            try
            {
                this.CheckReentrancy();

                index = this.items.Count;
                item = (T)value;

                this.items.Insert(index, item);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("'value' is the wrong type");
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyItemAdded(item, index);

            return index;
        }

        /// <summary>Removes all elements from the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        public void Clear()
        {
            this.itemsLocker.EnterWriteLock();

            try
            {
                this.CheckReentrancy();

                this.items.Clear();
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyCollectionReset();
        }

        /// <summary>Copies the <see cref="SynchronizedObservableCollection{T}" /> elements to an existing one-dimensional <see cref="System.Array" />, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="SynchronizedObservableCollection{T}" />. The <see cref="System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="SynchronizedObservableCollection{T}" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (!(arrayIndex >= 0 && arrayIndex < array.Length))
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (!(array.Length - arrayIndex >= this.Count))
                throw new ArgumentException("Invalid offset length.");

            this.itemsLocker.EnterReadLock();

            try
            {
                this.items.CopyTo(array, arrayIndex);
            }
            finally
            {
                this.itemsLocker.ExitReadLock();
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Rank != 1)
                throw new ArgumentException("Multidimensional array are not supported");

            if (!(arrayIndex >= 0 && arrayIndex < array.Length))
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("Non-zero lower bound is not supported");

            if (!(array.Length - arrayIndex >= this.Count))
                throw new ArgumentException("Invalid offset length.");

            this.itemsLocker.EnterReadLock();

            try
            {
                if (array is T[] tArray)
                {
                    this.items.CopyTo(tArray, arrayIndex);
                }
                else
                {
#if NETSTANDARD2_0 || NETFULL
                    // Catch the obvious case assignment will fail.
                    // We can found all possible problems by doing the check though.
                    // For example, if the element type of the Array is derived from T,
                    // we can't figure out if we can successfully copy the element beforehand.
                    var targetType = array.GetType().GetElementType();
                    var sourceType = typeof (T);
                    if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                    {
                        throw new ArrayTypeMismatchException("Invalid array type");
                    }
#endif

                    // We can't cast array of value type to object[], so we don't support 
                    // widening of primitive types here.
                    if (array is not object[] objects)
                    {
                        throw new ArrayTypeMismatchException("Invalid array type");
                    }

                    var count = this.items.Count;
                    try
                    {
                        for (var i = 0; i < count; i++)
                        {
                            objects[arrayIndex++] = this.items[i];
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArrayTypeMismatchException("Invalid array type");
                    }
                }
            }
            finally
            {
                this.itemsLocker.ExitReadLock();
            }
        }

        /// <summary>Determines whether an element is in the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="SynchronizedObservableCollection{T}" />; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
        public bool Contains(T item)
        {
            this.itemsLocker.EnterReadLock();

            try
            {
                return this.items.Contains(item);
            }
            finally
            {
                this.itemsLocker.ExitReadLock();
            }
        }

        bool IList.Contains(object value)
        {
            if (!IsCompatibleObject(value))
            {
                return false;
            }

            this.itemsLocker.EnterReadLock();

            try
            {
                return this.items.Contains((T)value);
            }
            finally
            {
                this.itemsLocker.ExitReadLock();
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="SynchronizedObservableCollection{T}"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Returns an enumerator that iterates through the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> for the <see cref="SynchronizedObservableCollection{T}" />.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            this.itemsLocker.EnterReadLock();

            try
            {
                return this.items.ToList().GetEnumerator();
            }
            finally
            {
                this.itemsLocker.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // ReSharper disable once RedundantCast
            return (IEnumerator)this.GetEnumerator();
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the entire <see cref="SynchronizedObservableCollection{T}" />, if found; otherwise, -1.</returns>
        /// <param name="item">The object to locate in the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
        public int IndexOf(T item)
        {
            this.itemsLocker.EnterReadLock();

            try
            {
                return this.items.IndexOf(item);
            }
            finally
            {
                this.itemsLocker.ExitReadLock();
            }
        }

        int IList.IndexOf(object value)
        {
            if (!IsCompatibleObject(value))
            {
                return -1;
            }

            this.itemsLocker.EnterReadLock();

            try
            {
                return this.items.IndexOf((T)value);
            }
            finally
            {
                this.itemsLocker.ExitReadLock();
            }
        }

        /// <summary>Inserts an element into the <see cref="SynchronizedObservableCollection{T}" /> at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="SynchronizedObservableCollection{T}.Count" />.</exception>
        public void Insert(int index, T item)
        {
            this.itemsLocker.EnterWriteLock();

            try
            {
                this.CheckReentrancy();

                if (index < 0 || index > this.items.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                this.items.Insert(index, item);
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyItemAdded(item, index);
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            this.itemsLocker.EnterWriteLock();

            try
            {
                this.CheckReentrancy();

                if (index < 0 || index > this.items.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                foreach (var item in items)
                {
                    this.items.Insert(index, item);
                    this.OnNotifyItemAdded(item, index++);
                }
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }
        }

        void IList.Insert(int index, object value)
        {
            try
            {
                this.Insert(index, (T)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("'value' is the wrong type");
            }
        }

        /// <summary>Moves the item at the specified index to a new location in the collection.</summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        public void Move(int oldIndex, int newIndex)
        {
            T item;

            this.itemsLocker.EnterWriteLock();

            try
            {
                this.CheckReentrancy();
                this.CheckIndex(oldIndex);
                this.CheckIndex(newIndex);

                item = this.items[oldIndex];

                this.items.RemoveAt(oldIndex);
                this.items.Insert(newIndex, item);
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyItemMoved(item, newIndex, oldIndex);
        }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false.  This method also returns false if <paramref name="item" /> was not found in the original <see cref="SynchronizedObservableCollection{T}" />.</returns>
        /// <param name="item">The object to remove from the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
        public bool Remove(T item)
        {
            int index;
            T value;

            this.itemsLocker.EnterWriteLock();

            try
            {
                this.CheckReentrancy();

                index = this.items.IndexOf(item);

                if (index < 0)
                {
                    return false;
                }

                value = this.items[index];

                this.items.RemoveAt(index);
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyItemRemoved(value, index);

            return true;
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                this.Remove((T)value);
            }
        }

        /// <summary>Removes the element at the specified index of the <see cref="SynchronizedObservableCollection{T}" />.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="SynchronizedObservableCollection{T}.Count" />.</exception>
        public void RemoveAt(int index)
        {
            T value;

            this.itemsLocker.EnterWriteLock();

            try
            {
                this.CheckIndex(index);
                this.CheckReentrancy();

                value = this.items[index];

                this.items.RemoveAt(index);
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }

            this.OnNotifyItemRemoved(value, index);
        }

        public void RemoveRange(int index, int count)
        {
            T value;
            this.itemsLocker.EnterWriteLock();
            try
            {
                this.CheckReentrancy();
                for (int i = index; i < index + count; i++)
                {
                    value = this.items[index];
                    this.items.RemoveAt(index);
                    this.OnNotifyItemRemoved(value, i);
                }
            }
            finally
            {
                this.itemsLocker.ExitWriteLock();
            }
        }

        private class SimpleMonitor : IDisposable
        {
            private int busyCount;

            public bool Busy => this.busyCount > 0;

            public void Enter()
            {
                ++this.busyCount;
            }

            public void Dispose()
            {
                --this.busyCount;
            }
        }
    }
}
