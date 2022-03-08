namespace TMP.UI.Controls.WPF.TreeListView
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using TMP.Shared;

    public class TreeListView : ListView
    {
        #region private

        private ITreeModel model;
        private TreeNode root;

        private ObservableCollections.ISynchronizedView<TreeNode, TreeNode> rowsView { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Internal collection of rows representing visible nodes, actually displayed in the ListView
        /// </summary>
        internal ObservableCollections.ObservableList<TreeNode> Rows
        {
            get;
            private set;
        }

        public ITreeModel Model
        {
            get => (ITreeModel)this.GetValue(ModelProperty);
            set => this.SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model), typeof(ITreeModel), typeof(TreeListView));

        internal TreeNode Root => this.root;

        public ReadOnlyCollection<TreeNode> Nodes => this.Root.Nodes;

        internal TreeNode PendingFocusNode
        {
            get;
            set;
        }

        public ICollection<TreeNode> SelectedNodes => this.SelectedItems.Cast<TreeNode>().ToArray();

        public TreeNode SelectedNode
        {
            get
            {
                if (this.SelectedItems.Count > 0)
                {
                    return this.SelectedItems[0] as TreeNode;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        public TreeListView()
        {
            this.Rows = new ObservableCollections.ObservableList<TreeNode>();
            this.root = new TreeNode(this, null);
            this.root.IsExpanded = true;

            this.rowsView = this.Rows.CreateView(i => i).WithINotifyCollectionChanged();
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(this.rowsView, new object());

            this.ItemsSource = this.rowsView;
            this.ItemContainerGenerator.StatusChanged += this.ItemContainerGeneratorStatusChanged;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ModelProperty)
            {
                this.model = (ITreeModel)e.NewValue;
                this.root.Children.Clear();
                this.Rows.Clear();
                this.CreateChildrenNodes(this.root);
            }
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && this.PendingFocusNode != null)
            {
                var item = this.ItemContainerGenerator.ContainerFromItem(this.PendingFocusNode) as TreeListViewItem;
                if (item != null)
                {
                    item.Focus();
                }

                this.PendingFocusNode = null;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is TreeListViewItem ti && item is TreeNode node)
            {
                ti.Node = node;
                base.PrepareContainerForItemOverride(element, node.Model);
            }
        }

        internal void SetIsExpanded(TreeNode node, bool value)
        {
            if (value)
            {
                if (!node.IsExpandedOnce)
                {
                    node.IsExpandedOnce = true;
                    node.AssignIsExpanded(value);
                    this.CreateChildrenNodes(node);
                }
                else
                {
                    node.AssignIsExpanded(value);
                    this.CreateChildrenRows(node);
                }
            }
            else
            {
                this.DropChildrenRows(node, false);
                node.AssignIsExpanded(value);
            }
        }

        internal void CreateChildrenNodes(TreeNode node)
        {
            var children = this.GetChildren(node);
            if (children != null)
            {
                int rowIndex = this.Rows.IndexOf(node);
                node.ChildrenSource = children as INotifyCollectionChanged;
                foreach (ITreeNode obj in children)
                {
                    TreeNode child = new (this, obj);
                    child.HasChildren = this.HasChildren(child);
                    node.Children.Add(child);
                }

                this.Rows.InsertRange(rowIndex + 1, node.Children.ToArray());
            }
        }

        private void CreateChildrenRows(TreeNode node)
        {
            int index = this.Rows.IndexOf(node);

            // ignore invisible nodes
            if (index >= 0 || node == this.root)
            {
                var nodes = node.AllVisibleChildren.ToArray();
                this.Rows.InsertRange(index + 1, nodes);
            }
        }

        internal void DropChildrenRows(TreeNode node, bool removeParent)
        {
            int start = this.Rows.IndexOf(node);

            // ignore invisible nodes
            if (start >= 0 || node == this.root)
            {
                int count = node.VisibleChildrenCount;
                if (removeParent)
                {
                    count++;
                }
                else
                {
                    start++;
                }

                this.Rows.RemoveRange(start, count);
            }
        }

        private IEnumerable GetChildren(TreeNode parent)
        {
            return this.model?.GetParentChildren(parent.Model);
        }

        private bool HasChildren(TreeNode parent)
        {
            return parent == this.Root || (this.model != null && this.model.HasParentChildren(parent.Model));
        }

        internal void InsertNewNode(TreeNode parent, ITreeNode model, int rowIndex, int index)
        {
            TreeNode node = new (this, model);
            if (index >= 0 && index < parent.Children.Count)
            {
                parent.Children.Insert(index, node);
            }
            else
            {
                index = parent.Children.Count;
                parent.Children.Add(node);
            }

            this.Rows.Insert(rowIndex + index + 1, node);
        }
    }
}
