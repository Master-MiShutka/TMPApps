namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using TMP.WORK.AramisChetchiki.Model;

    [Serializable]
    public abstract class BaseFieldsCollection : Collection<string>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public BaseFieldsCollection()
            : base()
        {

        }

        public BaseFieldsCollection(StringCollection list)
            : base()
        {
            // Workaround for VSWhidbey bug 562681 (tracked by Windows bug 1369339).
            // We should be able to simply call the base(list) ctor.  But Collection<T>
            // doesn't copy the list (contrary to the documentation) - it uses the
            // list directly as its storage.  So we do the copying here.
            this.CopyFrom(list);
        }

        public BaseFieldsCollection(IEnumerable<string> collection) : base()
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            this.CopyFrom(collection);
        }

        public BaseFieldsCollection(IEnumerable<Shared.PlusPropertyDescriptor> plusPropertyDescriptors)
        {
            IList<string> items = this.Items;
            if (plusPropertyDescriptors != null && items != null)
            {
                foreach (Shared.PlusPropertyDescriptor item in plusPropertyDescriptors)
                {
                    items.Add(item.Name);
                }
            }
        }

        private void CopyFrom(System.Collections.IEnumerable collection)
        {
            IList<string> items = this.Items;
            if (collection != null && items != null)
            {
                System.Collections.IEnumerator enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    items.Add((string)enumerator.Current);
                }
            }
        }

        #region Public Methods

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex)
        {
            this.MoveItem(oldIndex, newIndex);
        }

        public Shared.PlusPropertyDescriptorsCollection ToPlusPropertyDescriptorsCollection()
        {
            Shared.PlusPropertyDescriptorsCollection result = new Shared.PlusPropertyDescriptorsCollection();

            IDictionary<string, Shared.PlusPropertyDescriptor> dictionary = this.ModelType switch
            {
                Type t when t == typeof(Meter) => ModelHelper.MeterPropertiesCollection,
                Type t when t == typeof(ChangeOfMeter) => ModelHelper.ChangesOfMetersPropertiesCollection,
                Type t when t == typeof(ElectricitySupply) => ModelHelper.ElectricitySupplyPropertiesCollection,
                Type t when t == typeof(SummaryInfoItem) => ModelHelper.SummaryInfoItemPropertiesCollection,
                _ => throw new NotImplementedException($"Unknown type '{this.ModelType}'"),
            };

            IList<string> items = this.Items;
            foreach (string item in items)
            {
                if (dictionary.ContainsKey(item))
                {
                    result.Add(dictionary[item]);
                }
            }

            return result;
        }

        #endregion Public Methods

        #region Public Events

        //------------------------------------------------------
        #region INotifyPropertyChanged implementation
        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this.PropertyChanged += value;
            }

            remove
            {
                this.PropertyChanged -= value;
            }
        }
        #endregion INotifyPropertyChanged implementation

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion Public Events

        public abstract Type ModelType { get; }

        #region Protected Methods

        protected override void ClearItems()
        {
            this.CheckReentrancy();

            base.ClearItems();

            this.OnPropertyChanged(CountString);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionReset();
        }

        protected override void RemoveItem(int index)
        {
            this.CheckReentrancy();
            string removedItem = this[index];

            base.RemoveItem(index);

            this.OnPropertyChanged(CountString);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        protected override void InsertItem(int index, string item)
        {
            this.CheckReentrancy();
            base.InsertItem(index, item);

            this.OnPropertyChanged(CountString);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected override void SetItem(int index, string item)
        {
            this.CheckReentrancy();
            string originalItem = this[index];
            base.SetItem(index, item);

            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            this.CheckReentrancy();

            string removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);

            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }

        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                using (this.BlockReentrancy())
                {
                    this.CollectionChanged(this, e);
                }
            }
        }

        /// <summary>
        /// Disallow reentrant attempts to change this collection. E.g. a event handler
        /// of the CollectionChanged event is not allowed to make changes to this collection.
        /// </summary>
        /// <remarks>
        /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
        /// <code>
        ///         using (BlockReentrancy())
        ///         {
        ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
        ///         }
        /// </code>
        /// </remarks>
        protected IDisposable BlockReentrancy()
        {
            this.monitor.Enter();
            return this.monitor;
        }

        /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception>
        protected void CheckReentrancy()
        {
            if (this.monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if ((this.CollectionChanged != null) && (this.CollectionChanged.GetInvocationList().Length > 1))
                {
                    throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
                }
            }
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Helper to raise a PropertyChanged event  />).
        /// </summary>
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion Private Methods

        #region Private Types

        [Serializable()]
        private class SimpleMonitor : IDisposable
        {
            public void Enter()
            {
                ++this.busyCount;
            }

            public void Dispose()
            {
                --this.busyCount;
            }

            public bool Busy => this.busyCount > 0;

            private int busyCount;
        }

        #endregion Private Types

        #region Private Fields

        private const string CountString = "Count";

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";

        private SimpleMonitor monitor = new SimpleMonitor();

        #endregion Private Fields

    }

    public sealed class MeterFieldsCollection : BaseFieldsCollection
    {
        public MeterFieldsCollection()
            : base()
        {
        }

        public MeterFieldsCollection(IEnumerable<string> enumerable)
        {
            foreach (string item in enumerable)
            {
                this.Add(item);
            }
        }

        public MeterFieldsCollection(IEnumerable<Shared.PlusPropertyDescriptor> plusPropertyDescriptors)
            : base(plusPropertyDescriptors) { }

        public override Type ModelType => typeof(Model.Meter);
    }

    public sealed class ChangesOfMetersFieldsCollection : BaseFieldsCollection
    {
        public ChangesOfMetersFieldsCollection()
            : base()
        {
        }

        public ChangesOfMetersFieldsCollection(IEnumerable<string> enumerable)
        {
            foreach (string item in enumerable)
            {
                this.Add(item);
            }
        }

        public ChangesOfMetersFieldsCollection(IEnumerable<Shared.PlusPropertyDescriptor> plusPropertyDescriptors)
            : base(plusPropertyDescriptors) { }

        public override Type ModelType => typeof(Model.ChangeOfMeter);
    }

    public sealed class SummaryInfoFieldsCollection : BaseFieldsCollection
    {
        public SummaryInfoFieldsCollection()
            : base()
        {
        }

        public SummaryInfoFieldsCollection(IEnumerable<string> enumerable)
        {
            foreach (string item in enumerable)
            {
                this.Add(item);
            }
        }

        public SummaryInfoFieldsCollection(IEnumerable<Shared.PlusPropertyDescriptor> plusPropertyDescriptors)
            : base(plusPropertyDescriptors) { }

        public override Type ModelType => typeof(Model.SummaryInfoItem);
    }
}
