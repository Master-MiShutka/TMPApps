﻿namespace Interactivity
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;

    /// <summary>
    /// Represents a collection of IAttachedObject with a shared AssociatedObject and provides change notifications to its contents when that AssociatedObject changes.
    /// </summary>
    public abstract class AttachableCollection<T> : FreezableCollection<T>, IAttachedObject
        where T : DependencyObject, IAttachedObject
    {
        private Collection<T> snapshot;
        private DependencyObject associatedObject;

        /// <summary>
        /// The object on which the collection is hosted.
        /// </summary>
        protected DependencyObject AssociatedObject
        {
            get
            {
                base.ReadPreamble();
                return this.associatedObject;
            }
        }

        /// <summary>
        /// Gets the associated object.
        /// </summary>
        /// <value>The associated object.</value>
        DependencyObject IAttachedObject.AssociatedObject => this.AssociatedObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Interactivity.AttachableCollection`1" /> class.
        /// </summary>
        /// <remarks>Internal, because this should not be inherited outside this assembly.</remarks>
        internal AttachableCollection()
        {
            ((INotifyCollectionChanged)this).CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCollectionChanged);
            this.snapshot = new Collection<T>();
        }

        /// <summary>
        /// Called immediately after the collection is attached to an AssociatedObject.
        /// </summary>
        protected abstract void OnAttached();

        /// <summary>
        /// Called when the collection is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected abstract void OnDetaching();

        /// <summary>
        /// Called when a new item is added to the collection.
        /// </summary>
        /// <param name="item">The new item.</param>
        internal abstract void ItemAdded(T item);

        /// <summary>
        /// Called when an item is removed from the collection.
        /// </summary>
        /// <param name="item">The removed item.</param>
        internal abstract void ItemRemoved(T item);

        [Conditional("DEBUG")]
        private void VerifySnapshotIntegrity()
        {
            bool flag = base.Count == this.snapshot.Count;
            if (flag)
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if (base[i] != this.snapshot[i])
                    {
                        return;
                    }
                }
            }
        }

        /// <exception cref="T:System.InvalidOperationException">Cannot add the instance to a collection more than once.</exception>
        private void VerifyAdd(T item)
        {
            if (this.snapshot.Contains(item))
            {
                throw new InvalidOperationException(string.Format("Cannot add the same instance of {0} to a {1} more than once.", new object[]
                {
                    typeof(T).Name,
                    base.GetType().Name,
                }));
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        IEnumerator enumerator = e.NewItems.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                T t = (T)(object)enumerator.Current;
                                try
                                {
                                    this.VerifyAdd(t);
                                    this.ItemAdded(t);
                                }
                                finally
                                {
                                    this.snapshot.Insert(base.IndexOf(t), t);
                                }
                            }

                            return;
                        }
                        finally
                        {
                            IDisposable disposable = enumerator as IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }
                    }

                case NotifyCollectionChangedAction.Remove:
                    goto IL_13A;
                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Move:
                    return;

                case NotifyCollectionChangedAction.Reset:
                    goto IL_18D;
                default:
                    return;
            }

            IEnumerator enumerator2 = e.OldItems.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    T item = (T)(object)enumerator2.Current;
                    this.ItemRemoved(item);
                    this.snapshot.Remove(item);
                }
            }
            finally
            {
                IDisposable disposable2 = enumerator2 as IDisposable;
                if (disposable2 != null)
                {
                    disposable2.Dispose();
                }
            }

            IEnumerator enumerator3 = e.NewItems.GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    T t2 = (T)(object)enumerator3.Current;
                    try
                    {
                        this.VerifyAdd(t2);
                        this.ItemAdded(t2);
                    }
                    finally
                    {
                        this.snapshot.Insert(base.IndexOf(t2), t2);
                    }
                }

                return;
            }
            finally
            {
                IDisposable disposable3 = enumerator3 as IDisposable;
                if (disposable3 != null)
                {
                    disposable3.Dispose();
                }
            }

        IL_13A:
            IEnumerator enumerator4 = e.OldItems.GetEnumerator();
            try
            {
                while (enumerator4.MoveNext())
                {
                    T item2 = (T)(object)enumerator4.Current;
                    this.ItemRemoved(item2);
                    this.snapshot.Remove(item2);
                }

                return;
            }
            finally
            {
                IDisposable disposable4 = enumerator4 as IDisposable;
                if (disposable4 != null)
                {
                    disposable4.Dispose();
                }
            }

        IL_18D:
            foreach (T current in this.snapshot)
            {
                this.ItemRemoved(current);
            }

            this.snapshot = new Collection<T>();
            foreach (T current2 in this)
            {
                this.VerifyAdd(current2);
                this.ItemAdded(current2);
            }
        }

        /// <summary>
        /// Attaches to the specified object.
        /// </summary>
        /// <param name="dependencyObject">The object to attach to.</param>
        /// <exception cref="T:System.InvalidOperationException">The IAttachedObject is already attached to a different object.</exception>
        public void Attach(DependencyObject dependencyObject)
        {
            if (dependencyObject != this.AssociatedObject)
            {
                if (this.AssociatedObject != null)
                {
                    throw new InvalidOperationException();
                }

                if (!(bool)base.GetValue(DesignerProperties.IsInDesignModeProperty))
                {
                    base.WritePreamble();
                    this.associatedObject = dependencyObject;
                    base.WritePostscript();
                }

                this.OnAttached();
            }
        }

        /// <summary>
        /// Detaches this instance from its associated object.
        /// </summary>
        public void Detach()
        {
            this.OnDetaching();
            base.WritePreamble();
            this.associatedObject = null;
            base.WritePostscript();
        }
    }
}