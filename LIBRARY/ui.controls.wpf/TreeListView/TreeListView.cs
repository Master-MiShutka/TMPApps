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
        private ObservableCollections.ISynchronizedView<TreeNode, TreeNode> rowsView;

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

        /// <summary>
        /// Data model, which used to build tree
        /// </summary>
        public ITreeModel Model
        {
            get => (ITreeModel)this.GetValue(ModelProperty);
            set => this.SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model), typeof(ITreeModel), typeof(TreeListView));

        /// <summary>
        /// Root node of tree
        /// </summary>
        internal TreeNode Root => this.root;

        /// <summary>
        /// Nodes of tree
        /// </summary>
        public ReadOnlyCollection<TreeNode> Nodes => this.Root.Nodes;

        /// <summary>
        /// Focused node
        /// </summary>
        internal TreeNode PendingFocusNode
        {
            get;
            set;
        }

        public ICollection<TreeNode> SelectedNodes => this.SelectedItems.Cast<TreeNode>().ToArray();

        /// <summary>
        /// Selected node in tree
        /// </summary>
        public TreeNode SelectedNode => this.SelectedItems.Count > 0 ? this.SelectedItems[0] as TreeNode : null;

        /// <summary>
        /// Count of tree visible nodes
        /// </summary>
        public int VisibleNodesCount
        {
            get => (int)this.GetValue(VisibleNodesCountProperty);
            private set => this.SetValue(VisibleNodesCountProperty, value);
        }

        public static readonly DependencyProperty VisibleNodesCountProperty = DependencyProperty.Register(
              nameof(VisibleNodesCount), typeof(int), typeof(TreeListView));

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

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && this.PendingFocusNode != null)
            {
                if (this.ItemContainerGenerator.ContainerFromItem(this.PendingFocusNode) is TreeListViewItem item)
                {
                    item.Focus();
                }

                this.PendingFocusNode = null;
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

        private IEnumerable GetChildren(TreeNode parent)
        {
            return this.model?.GetParentChildren(parent.Model);
        }

        private bool HasChildren(TreeNode parent)
        {
            return parent == this.Root || (this.model != null && this.model.HasParentChildren(parent.Model));
        }

        internal void OnNodePropertyChanged()
        {
            this.VisibleNodesCount = (this.Rows == null) ? 0 : this.Rows.Count(n => n.IsMatch);
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
