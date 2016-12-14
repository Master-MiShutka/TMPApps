using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.ComponentModel;

namespace TMP.ARMTES
{
    /// <summary>
    /// An ObservableCollection&lt;T&gt; enhanced with capability of free threading.
    /// </summary>
    [Serializable]
    public class BindableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the<see cref="BindingCollection&lt;T&gt;">BindingCollection</see>.
        /// </summary>
        public BindableCollection() : base() { }

        /// <summary>
        /// Initializes a new instance of the<see cref="BindingCollection&lt;T&gt;">BindingCollection</see>
        /// class that contains elements copied from the specified List&lt;T&gt;.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        /// <exception cref="System.ArgumentNullException">The list parameter cannot be null.</exception>
        public BindableCollection(List<T> list) : base(list) { }

        /// <summary>
        /// Initializes a new instance of the<see cref="BindingCollection&lt;T&gt;">BindingCollection</see>
        /// class that contains elements copied from the specified IEnumerable&lt;T&gt;.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        /// <exception cref="System.ArgumentNullException">The list parameter cannot be null.</exception>
        public BindableCollection(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            foreach (var item in list)
            {
                Items.Add(item);
            }
        }

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var collectionChanged = CollectionChanged;

            if (collectionChanged != null)
            {
                using (var blockReentrancy = BlockReentrancy())
                {
                    foreach (var @delegate in collectionChanged.GetInvocationList())
                    {
                        var dispatcherInvoker = @delegate.Target as DispatcherObject;
                        var syncInvoker = @delegate.Target as ISynchronizeInvoke;
                        if (dispatcherInvoker != null)
                        {
                            // We are running inside DispatcherSynchronizationContext,
                            // so we should invoke the event handler in the correct dispatcher.
                            dispatcherInvoker.Dispatcher.Invoke(@delegate, DispatcherPriority.Background, this, e);
                        }
                        else if (syncInvoker != null)
                        {
                            // We are running inside WindowsFormsSynchronizationContext,
                            // so we should invoke the event handler in the correct context.
                            syncInvoker.Invoke(@delegate, new object[] { this, e });
                        }
                        else
                        {
                            // We are running in free threaded context, so just directly invoke the event handler.
                            var handler = (NotifyCollectionChangedEventHandler)@delegate;
                            handler(this, e);
                        }
                    }
                }
            }
        }
    }
}
