// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Collection that validates that inserted nodes do not have another parent.
    /// </summary>
    public sealed class SharpTreeNodeCollection : IList<SharpTreeNode>, INotifyCollectionChanged
    {
        private readonly SharpTreeNode parent;
        private List<SharpTreeNode> list = new List<SharpTreeNode>();
        private bool isRaisingEvent;

        public SharpTreeNodeCollection(SharpTreeNode parent)
        {
            this.parent = parent;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(!this.isRaisingEvent);
            this.isRaisingEvent = true;
            try
            {
                this.parent.OnChildrenChanged(e);
                if (this.CollectionChanged != null)
                {
                    this.CollectionChanged(this, e);
                }
            }
            finally
            {
                this.isRaisingEvent = false;
            }
        }

        private void ThrowOnReentrancy()
        {
            if (this.isRaisingEvent)
            {
                throw new InvalidOperationException();
            }
        }

        private void ThrowIfValueIsNullOrHasParent(SharpTreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (node.modelParent != null)
            {
                throw new ArgumentException("The node already has a parent", nameof(node));
            }
        }

        public SharpTreeNode this[int index]
        {
            get => this.list[index];

            set
            {
                this.ThrowOnReentrancy();
                var oldItem = this.list[index];
                if (oldItem == value)
                {
                    return;
                }

                this.ThrowIfValueIsNullOrHasParent(value);
                this.list[index] = value;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        public int Count => this.list.Count;

        bool ICollection<SharpTreeNode>.IsReadOnly => false;

        public int IndexOf(SharpTreeNode node)
        {
            if (node == null || node.modelParent != this.parent)
            {
                return -1;
            }
            else
            {
                return this.list.IndexOf(node);
            }
        }

        /// <summary>
        /// Почему-то IndexOf возвращает -1
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public int GetIndexOf(SharpTreeNode node)
        {
            if (node == null || node.modelParent != this.parent)
            {
                return -1;
            }
            else
            {
                return this.list.FindIndex(i => i == node);
            }
        }

        public void Insert(int index, SharpTreeNode node)
        {
            this.ThrowOnReentrancy();
            this.ThrowIfValueIsNullOrHasParent(node);
            this.list.Insert(index, node);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index));
        }

        public void InsertRange(int index, IEnumerable<SharpTreeNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            this.ThrowOnReentrancy();
            List<SharpTreeNode> newNodes = nodes.ToList();
            if (newNodes.Count == 0)
            {
                return;
            }

            foreach (SharpTreeNode node in newNodes)
            {
                this.ThrowIfValueIsNullOrHasParent(node);
            }

            this.list.InsertRange(index, newNodes);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNodes, index));
        }

        public void RemoveAt(int index)
        {
            this.ThrowOnReentrancy();
            var oldItem = this.list[index];
            this.list.RemoveAt(index);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
        }

        public void RemoveRange(int index, int count)
        {
            this.ThrowOnReentrancy();
            if (count == 0)
            {
                return;
            }

            var oldItems = this.list.GetRange(index, count);
            this.list.RemoveRange(index, count);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, index));
        }

        public void Add(SharpTreeNode node)
        {
            this.ThrowOnReentrancy();
            this.ThrowIfValueIsNullOrHasParent(node);
            this.list.Add(node);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, this.list.Count - 1));
        }

        public void AddRange(IEnumerable<SharpTreeNode> nodes)
        {
            this.InsertRange(this.Count, nodes);
        }

        public void Clear()
        {
            this.ThrowOnReentrancy();
            var oldList = this.list;
            this.list = new List<SharpTreeNode>();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldList, 0));
        }

        public bool Contains(SharpTreeNode node)
        {
            return this.IndexOf(node) >= 0;
        }

        public void CopyTo(SharpTreeNode[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public bool Remove(SharpTreeNode item)
        {
            int pos = this.IndexOf(item);
            if (pos >= 0)
            {
                this.RemoveAt(pos);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator<SharpTreeNode> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public void RemoveAll(Predicate<SharpTreeNode> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            this.ThrowOnReentrancy();
            int firstToRemove = 0;
            for (int i = 0; i < this.list.Count; i++)
            {
                bool removeNode;
                this.isRaisingEvent = true;
                try
                {
                    removeNode = match(this.list[i]);
                }
                finally
                {
                    this.isRaisingEvent = false;
                }

                if (!removeNode)
                {
                    if (firstToRemove < i)
                    {
                        this.RemoveRange(firstToRemove, i - firstToRemove);
                        i = firstToRemove - 1;
                    }
                    else
                    {
                        firstToRemove = i + 1;
                    }

                    Debug.Assert(firstToRemove == i + 1);
                }
            }

            if (firstToRemove < this.list.Count)
            {
                this.RemoveRange(firstToRemove, this.list.Count - firstToRemove);
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            SharpTreeNode item = this[oldIndex];
            this.RemoveAt(oldIndex);
            this.Insert(newIndex, item);
        }
    }
}
