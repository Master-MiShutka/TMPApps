namespace TMP.UI.Controls.WPF.TreeListView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    public sealed class TreeNode : TMP.Shared.PropertyChangedBase, Shared.IChildItem<TreeNode>
    {
        #region Properties

        internal TreeListView Tree { get; private set; }

        private INotifyCollectionChanged childrenSource;

        internal INotifyCollectionChanged ChildrenSource
        {
            get => this.childrenSource;

            set
            {
                if (this.childrenSource != null)
                {
                    this.childrenSource.CollectionChanged -= this.ChildrenChanged;
                }

                this.childrenSource = value;

                if (this.childrenSource != null)
                {
                    this.childrenSource.CollectionChanged += this.ChildrenChanged;
                }
            }
        }

        public int Index { get; private set; } = -1;

        /// <summary>
        /// Returns true if all parent nodes of this node are expanded.
        /// </summary>
        internal bool IsVisible
        {
            get
            {
                TreeNode node = this.Parent;
                while (node != null)
                {
                    if (!node.IsExpanded)
                    {
                        return false;
                    }

                    node = node.Parent;
                }

                return true;
            }
        }

        public bool IsExpandedOnce
        {
            get;
            internal set;
        }

        public bool HasChildren
        {
            get;
            internal set;
        }

        private bool isExpanded = false;

        public bool IsExpanded
        {
            get => this.isExpanded;

            set
            {
                if (value != this.IsExpanded)
                {
                    this.Tree.SetIsExpanded(this, value);
                    this.RaisePropertyChanged(nameof(this.IsExpanded));
                    this.RaisePropertyChanged(nameof(this.IsExpandable));
                }
            }
        }

        internal void AssignIsExpanded(bool value)
        {
            this.isExpanded = value;
        }

        public bool IsExpandable => (this.HasChildren && !this.IsExpandedOnce) || this.Nodes.Count > 0;

        private bool isSelected;

        public bool IsSelected
        {
            get => this.isSelected;

            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    this.RaisePropertyChanged(nameof(this.IsSelected));
                }
            }
        }

        public TreeNode Parent { get; set; }

        public int Level
        {
            get
            {
                if (this.Parent == null)
                {
                    return -1;
                }
                else
                {
                    return this.Parent.Level + 1;
                }
            }
        }

        public TreeNode PreviousNode
        {
            get
            {
                if (this.Parent != null)
                {
                    int index = this.Index;
                    if (index > 0)
                    {
                        return this.Parent.Nodes[index - 1];
                    }
                }

                return null;
            }
        }

        public TreeNode NextNode
        {
            get
            {
                if (this.Parent != null)
                {
                    int index = this.Index;
                    if (index < this.Parent.Nodes.Count - 1)
                    {
                        return this.Parent.Nodes[index + 1];
                    }
                }

                return null;
            }
        }

        internal TreeNode BottomNode
        {
            get
            {
                TreeNode parent = this.Parent;
                if (parent != null)
                {
                    if (parent.NextNode != null)
                    {
                        return parent.NextNode;
                    }
                    else
                    {
                        return parent.BottomNode;
                    }
                }

                return null;
            }
        }

        internal TreeNode NextVisibleNode
        {
            get
            {
                if (this.IsExpanded && this.Nodes.Count > 0)
                {
                    return this.Nodes[0];
                }
                else
                {
                    TreeNode nn = this.NextNode;
                    if (nn != null)
                    {
                        return nn;
                    }
                    else
                    {
                        return this.BottomNode;
                    }
                }
            }
        }

        public int VisibleChildrenCount => this.AllVisibleChildren.Count();

        public IEnumerable<TreeNode> AllVisibleChildren
        {
            get
            {
                int level = this.Level;
                TreeNode node = this;
                while (true)
                {
                    node = node.NextVisibleNode;
                    if (node != null && node.Level > level)
                    {
                        yield return node;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public object Tag { get; private set; }

        internal Collection<TreeNode> Children { get; private set; }

        public ReadOnlyCollection<TreeNode> Nodes { get; private set; }

        #endregion

        internal TreeNode(TreeListView tree, object tag)
        {
            this.Tree = tree ?? throw new ArgumentNullException(nameof(tree));
            this.Children = new Shared.ChildItemCollection<TreeNode>(this);
            this.Nodes = new ReadOnlyCollection<TreeNode>(this.Children);
            this.Tag = tag;
        }

        public override string ToString()
        {
            if (this.Tag != null)
            {
                return this.Tag.ToString();
            }
            else
            {
                return base.ToString();
            }
        }

        private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        int index = e.NewStartingIndex;
                        int rowIndex = this.Tree.Rows.IndexOf(this);
                        foreach (object obj in e.NewItems)
                        {
                            this.Tree.InsertNewNode(this, obj, rowIndex, index);
                            index++;
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (this.Children.Count > e.OldStartingIndex)
                    {
                        this.RemoveChildAt(e.OldStartingIndex);
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    while (this.Children.Count > 0)
                    {
                        this.RemoveChildAt(0);
                    }

                    this.Tree.CreateChildrenNodes(this);
                    break;
            }

            this.HasChildren = this.Children.Count > 0;
            this.RaisePropertyChanged(nameof(this.IsExpandable));
        }

        private void RemoveChildAt(int index)
        {
            var child = this.Children[index];
            this.Tree.DropChildrenRows(child, true);
            this.ClearChildrenSource(child);
            this.Children.RemoveAt(index);
        }

        private void ClearChildrenSource(TreeNode node)
        {
            node.ChildrenSource = null;
            foreach (var n in node.Children)
            {
                this.ClearChildrenSource(n);
            }
        }
    }
}
