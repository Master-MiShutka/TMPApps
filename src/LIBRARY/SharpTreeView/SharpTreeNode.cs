// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    public partial class SharpTreeNode : INotifyPropertyChanged
    {
        private SharpTreeNodeCollection modelChildren;
        internal SharpTreeNode modelParent;
        private bool isVisible = true;
        private bool ismatch = true;

        private void UpdateIsVisible(bool parentIsVisible, bool updateFlattener)
        {
            bool newIsVisible = parentIsVisible && !this.isHidden;
            if (this.isVisible != newIsVisible)
            {
                this.isVisible = newIsVisible;

                // invalidate the augmented data
                SharpTreeNode node = this;
                while (node != null && node.totalListLength >= 0)
                {
                    node.totalListLength = -1;
                    node = node.listParent;
                }

                // Remember the removed nodes:
                List<SharpTreeNode> removedNodes = null;
                if (updateFlattener && !newIsVisible)
                {
                    removedNodes = this.VisibleDescendantsAndSelf().ToList();
                }

                // also update the model children:
                this.UpdateChildIsVisible(false);

                // Validate our invariants:
                if (updateFlattener)
                {
                    this.CheckRootInvariants();
                }

                // Tell the flattener about the removed nodes:
                if (removedNodes != null)
                {
                    var flattener = this.GetListRoot().treeFlattener;
                    if (flattener != null)
                    {
                        flattener.NodesRemoved(GetVisibleIndexForNode(this), removedNodes);
                        foreach (var n in removedNodes)
                        {
                            n.OnIsVisibleChanged();
                        }
                    }
                }

                // Tell the flattener about the new nodes:
                if (updateFlattener && newIsVisible)
                {
                    var flattener = this.GetListRoot().treeFlattener;
                    if (flattener != null)
                    {
                        flattener.NodesInserted(GetVisibleIndexForNode(this), this.VisibleDescendantsAndSelf());
                        foreach (var n in this.VisibleDescendantsAndSelf())
                        {
                            n.OnIsVisibleChanged();
                        }
                    }
                }
            }
        }

        protected virtual void OnIsVisibleChanged()
        {
        }

        private void UpdateChildIsVisible(bool updateFlattener)
        {
            if (this.modelChildren != null && this.modelChildren.Count > 0)
            {
                bool showChildren = this.isVisible && this.isExpanded;
                foreach (SharpTreeNode child in this.modelChildren)
                {
                    child.UpdateIsVisible(showChildren, updateFlattener);
                }
            }
        }

        #region Main

        public SharpTreeNode()
        {
        }

        public SharpTreeNodeCollection Children
        {
            get
            {
                if (this.modelChildren == null)
                {
                    this.modelChildren = new SharpTreeNodeCollection(this);
                }

                return this.modelChildren;
            }
        }

        public SharpTreeNode Parent => this.modelParent;

        public virtual object Text => null;

        public virtual int ChildrenCount => this.Children.Count;

        public virtual Brush Foreground => SystemColors.WindowTextBrush;

        public virtual object Icon => null;

        public virtual object ToolTip => null;

        public int Level => this.Parent != null ? this.Parent.Level + 1 : 0;

        public bool IsRoot => this.Parent == null;

        private bool isHidden;

        public bool IsHidden
        {
            get => this.isHidden;

            set
            {
                if (this.isHidden != value)
                {
                    this.isHidden = value;
                    if (this.modelParent != null)
                    {
                        this.UpdateIsVisible(this.modelParent.isVisible && this.modelParent.isExpanded, true);
                    }

                    this.RaisePropertyChanged("IsHidden");
                    if (this.Parent != null)
                    {
                        this.Parent.RaisePropertyChanged("ShowExpander");
                    }
                }
            }
        }

        /// <summary>
        /// Return true when this node is not hidden and when all parent nodes are expanded and not hidden.
        /// </summary>
        public bool IsVisible => this.isVisible;

        private bool isSelected;

        public bool IsSelected
        {
            get => this.isSelected;

            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.RaisePropertyChanged("IsSelected");
                }
            }
        }

        private object tag;

        public object Tag
        {
            get => this.tag;

            set
            {
                if (this.tag != value)
                {
                    this.tag = value;
                    this.RaisePropertyChanged("Tag");
                }
            }
        }

        #endregion

        #region OnChildrenChanged
        internal protected virtual void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SharpTreeNode node in e.OldItems)
                {
                    Debug.Assert(node.modelParent == this);
                    node.modelParent = null;
                    Debug.WriteLine("Removing {0} from {1}", node, this);
                    SharpTreeNode removeEnd = node;
                    while (removeEnd.modelChildren != null && removeEnd.modelChildren.Count > 0)
                    {
                        removeEnd = removeEnd.modelChildren.Last();
                    }

                    List<SharpTreeNode> removedNodes = null;
                    int visibleIndexOfRemoval = 0;
                    if (node.isVisible)
                    {
                        visibleIndexOfRemoval = GetVisibleIndexForNode(node);
                        removedNodes = node.VisibleDescendantsAndSelf().ToList();
                    }

                    this.RemoveNodes(node, removeEnd);

                    if (removedNodes != null)
                    {
                        var flattener = this.GetListRoot().treeFlattener;
                        if (flattener != null)
                        {
                            flattener.NodesRemoved(visibleIndexOfRemoval, removedNodes);
                        }
                    }
                }
            }

            if (e.NewItems != null)
            {
                SharpTreeNode insertionPos;
                if (e.NewStartingIndex == 0)
                {
                    insertionPos = null;
                }
                else
                {
                    insertionPos = this.modelChildren[e.NewStartingIndex - 1];
                }

                foreach (SharpTreeNode node in e.NewItems)
                {
                    Debug.Assert(node.modelParent == null);
                    node.modelParent = this;
                    node.UpdateIsVisible(this.isVisible && this.isExpanded, false);

                    // Debug.WriteLine("Inserting {0} after {1}", node, insertionPos);
                    while (insertionPos != null && insertionPos.modelChildren != null && insertionPos.modelChildren.Count > 0)
                    {
                        insertionPos = insertionPos.modelChildren.Last();
                    }

                    InsertNodeAfter(insertionPos ?? this, node);

                    insertionPos = node;
                    if (node.isVisible)
                    {
                        var flattener = this.GetListRoot().treeFlattener;
                        if (flattener != null)
                        {
                            flattener.NodesInserted(GetVisibleIndexForNode(node), node.VisibleDescendantsAndSelf());
                        }
                    }
                }
            }

            this.RaisePropertyChanged("ShowExpander");
            this.RaiseIsLastChangedIfNeeded(e);
        }
        #endregion

        #region IsMatch

        public bool IsMatch
        {
            get => this.ismatch;

            set
            {
                if (value == this.ismatch)
                {
                    return;
                }

                this.ismatch = value;
                this.RaisePropertyChanged("IsMatch");
            }
        }

        public virtual bool IsCriteriaMatched(string criteria)
        {
            string text = this.Text == null ? string.Empty : this.Text.ToString();

            return string.IsNullOrEmpty(criteria) || text.Contains(criteria);
        }

        public void ApplyCriteria(string criteria, Stack<SharpTreeNode> ancestors)
        {
            if (this.IsCriteriaMatched(criteria))
            {
                this.IsMatch = true;
                foreach (var ancestor in ancestors)
                {
                    ancestor.IsMatch = true;
                    ancestor.IsExpanded = !string.IsNullOrEmpty(criteria);
                }
            }
            else
            {
                this.IsMatch = false;
            }

            ancestors.Push(this);
            foreach (var child in this.Children)
            {
                child.ApplyCriteria(criteria, ancestors);
            }

            ancestors.Pop();
        }

        #endregion

        #region Expanding / LazyLoading

        public virtual object ExpandedIcon => this.Icon;

        public virtual bool ShowExpander => this.LazyLoading || this.Children.Any(c => !c.isHidden);

        private bool isExpanded;

        public bool IsExpanded
        {
            get => this.isExpanded;

            set
            {
                if (this.isExpanded != value)
                {
                    this.isExpanded = value;
                    if (this.isExpanded)
                    {
                        this.EnsureLazyChildren();
                        this.OnExpanding();
                        foreach (var child in this.Children)
                        {
                            child.IsMatch = true;
                        }
                    }
                    else
                    {
                        this.OnCollapsing();
                    }

                    this.UpdateChildIsVisible(true);
                    this.RaisePropertyChanged("IsExpanded");
                }
            }
        }

        protected virtual void OnExpanding()
        {
        }

        protected virtual void OnCollapsing()
        {
        }

        private bool lazyLoading;

        public bool LazyLoading
        {
            get => this.lazyLoading;

            set
            {
                this.lazyLoading = value;
                if (this.lazyLoading)
                {
                    this.IsExpanded = false;
                    if (this.canExpandRecursively)
                    {
                        this.canExpandRecursively = false;
                        this.RaisePropertyChanged("CanExpandRecursively");
                    }
                }

                this.RaisePropertyChanged("LazyLoading");
                this.RaisePropertyChanged("ShowExpander");
            }
        }

        private bool canExpandRecursively = true;

        /// <summary>
        /// Gets whether this node can be expanded recursively.
        /// If not overridden, this property returns false if the node is using lazy-loading, and true otherwise.
        /// </summary>
        public virtual bool CanExpandRecursively => this.canExpandRecursively;

        public virtual bool ShowIcon => this.Icon != null;

        protected virtual void LoadChildren()
        {
            throw new NotSupportedException(this.GetType().Name + " does not support lazy loading");
        }

        /// <summary>
        /// Ensures the children were initialized (loads children if lazy loading is enabled)
        /// </summary>
        public void EnsureLazyChildren()
        {
            if (this.LazyLoading)
            {
                this.LazyLoading = false;
                this.LoadChildren();
            }
        }

        #endregion

        #region Ancestors / Descendants

        public IEnumerable<SharpTreeNode> Descendants()
        {
            return TreeTraversal.PreOrder(this.Children, n => n.Children);
        }

        public IEnumerable<SharpTreeNode> DescendantsAndSelf()
        {
            return TreeTraversal.PreOrder(this, n => n.Children);
        }

        internal IEnumerable<SharpTreeNode> VisibleDescendants()
        {
            return TreeTraversal.PreOrder(this.Children.Where(c => c.isVisible), n => n.Children.Where(c => c.isVisible));
        }

        internal IEnumerable<SharpTreeNode> VisibleDescendantsAndSelf()
        {
            return TreeTraversal.PreOrder(this, n => n.Children.Where(c => c.isVisible));
        }

        public IEnumerable<SharpTreeNode> Ancestors()
        {
            var node = this;
            while (node.Parent != null)
            {
                yield return node.Parent;
                node = node.Parent;
            }
        }

        public IEnumerable<SharpTreeNode> AncestorsAndSelf()
        {
            yield return this;
            foreach (var node in this.Ancestors())
            {
                yield return node;
            }
        }

        #endregion

        #region Editing

        public virtual bool IsEditable => false;

        private bool isEditing;

        public bool IsEditing
        {
            get => this.isEditing;

            set
            {
                if (this.isEditing != value)
                {
                    this.isEditing = value;
                    this.RaisePropertyChanged("IsEditing");
                }
            }
        }

        public virtual string LoadEditText()
        {
            return null;
        }

        public virtual bool SaveEditText(string value)
        {
            return true;
        }

        #endregion

        #region Checkboxes

        public virtual bool IsCheckable => false;

        private bool? isChecked;

        public bool? IsChecked
        {
            get => this.isChecked;

            set => this.SetIsChecked(value, true);
        }

        private void SetIsChecked(bool? value, bool update)
        {
            if (this.isChecked != value)
            {
                this.isChecked = value;

                if (update)
                {
                    if (this.IsChecked != null)
                    {
                        foreach (var child in this.Descendants())
                        {
                            if (child.IsCheckable)
                            {
                                child.SetIsChecked(this.IsChecked, false);
                            }
                        }
                    }

                    foreach (var parent in this.Ancestors())
                    {
                        if (parent.IsCheckable)
                        {
                            if (!parent.TryValueForIsChecked(true))
                            {
                                if (!parent.TryValueForIsChecked(false))
                                {
                                    parent.SetIsChecked(null, false);
                                }
                            }
                        }
                    }
                }

                this.RaisePropertyChanged("IsChecked");
            }
        }

        private bool TryValueForIsChecked(bool? value)
        {
            if (this.Children.Where(n => n.IsCheckable).All(n => n.IsChecked == value))
            {
                this.SetIsChecked(value, false);
                return true;
            }

            return false;
        }

        #endregion

        #region Cut / Copy / Paste / Delete

        public bool IsCut => false;

        /*
			static List<SharpTreeNode> cuttedNodes = new List<SharpTreeNode>();
			static IDataObject cuttedData;
			static EventHandler requerySuggestedHandler; // for weak event

			static void StartCuttedDataWatcher()
			{
				requerySuggestedHandler = new EventHandler(CommandManager_RequerySuggested);
				CommandManager.RequerySuggested += requerySuggestedHandler;
			}

			static void CommandManager_RequerySuggested(object sender, EventArgs e)
			{
				if (cuttedData != null && !Clipboard.IsCurrent(cuttedData)) {
					ClearCuttedData();
				}
			}

			static void ClearCuttedData()
			{
				foreach (var node in cuttedNodes) {
					node.IsCut = false;
				}
				cuttedNodes.Clear();
				cuttedData = null;
			}

			//static public IEnumerable<SharpTreeNode> PurifyNodes(IEnumerable<SharpTreeNode> nodes)
			//{
			//    var list = nodes.ToList();
			//    var array = list.ToArray();
			//    foreach (var node1 in array) {
			//        foreach (var node2 in array) {
			//            if (node1.Descendants().Contains(node2)) {
			//                list.Remove(node2);
			//            }
			//        }
			//    }
			//    return list;
			//}

			bool isCut;

			public bool IsCut
			{
				get { return isCut; }
				private set
				{
					isCut = value;
					RaisePropertyChanged("IsCut");
				}
			}

			internal bool InternalCanCut()
			{
				return InternalCanCopy() && InternalCanDelete();
			}

			internal void InternalCut()
			{
				ClearCuttedData();
				cuttedData = Copy(ActiveNodesArray);
				Clipboard.SetDataObject(cuttedData);

				foreach (var node in ActiveNodes) {
					node.IsCut = true;
					cuttedNodes.Add(node);
				}
			}

			internal bool InternalCanCopy()
			{
				return CanCopy(ActiveNodesArray);
			}

			internal void InternalCopy()
			{
				Clipboard.SetDataObject(Copy(ActiveNodesArray));
			}

			internal bool InternalCanPaste()
			{
				return CanPaste(Clipboard.GetDataObject());
			}

			internal void InternalPaste()
			{
				Paste(Clipboard.GetDataObject());

				if (cuttedData != null) {
					DeleteCore(cuttedNodes.ToArray());
					ClearCuttedData();
				}
			}
		 */

        public virtual bool CanDelete()
        {
            return false;
        }

        public virtual void Delete()
        {
            throw new NotSupportedException(this.GetType().Name + " does not support deletion");
        }

        public virtual void DeleteCore()
        {
            throw new NotSupportedException(this.GetType().Name + " does not support deletion");
        }

        public virtual IDataObject Copy(SharpTreeNode[] nodes)
        {
            throw new NotSupportedException(this.GetType().Name + " does not support copy/paste or drag'n'drop");
        }

        /*
			public virtual bool CanCopy(SharpTreeNode[] nodes)
			{
				return false;
			}

			public virtual bool CanPaste(IDataObject data)
			{
				return false;
			}

			public virtual void Paste(IDataObject data)
			{
				EnsureLazyChildren();
				Drop(data, Children.Count, DropEffect.Copy);
			}
		 */
        #endregion

        #region Drag and Drop
        public virtual bool CanDrag(SharpTreeNode[] nodes)
        {
            return false;
        }

        public virtual void StartDrag(DependencyObject dragSource, SharpTreeNode[] nodes)
        {
            DragDropEffects effects = DragDropEffects.All;
            if (!nodes.All(n => n.CanDelete()))
            {
                effects &= ~DragDropEffects.Move;
            }

            DragDropEffects result = DragDrop.DoDragDrop(dragSource, this.Copy(nodes), effects);
            if (result == DragDropEffects.Move)
            {
                foreach (SharpTreeNode node in nodes)
                {
                    node.DeleteCore();
                }
            }
        }

        public virtual bool CanDrop(DragEventArgs e, int index)
        {
            return false;
        }

        internal void InternalDrop(DragEventArgs e, int index)
        {
            if (this.LazyLoading)
            {
                this.EnsureLazyChildren();
                index = this.Children.Count;
            }

            this.Drop(e, index);
        }

        public virtual void Drop(DragEventArgs e, int index)
        {
            throw new NotSupportedException(this.GetType().Name + " does not support Drop()");
        }
        #endregion

        #region IsLast (for TreeView lines)

        public bool IsLast => this.Parent == null ||
                    this.Parent.Children[this.Parent.Children.Count - 1] == this;

        private void RaiseIsLastChangedIfNeeded(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex == this.Children.Count - 1)
                    {
                        if (this.Children.Count > 1)
                        {
                            this.Children[this.Children.Count - 2].RaisePropertyChanged("IsLast");
                        }

                        this.Children[this.Children.Count - 1].RaisePropertyChanged("IsLast");
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex == this.Children.Count)
                    {
                        if (this.Children.Count > 0)
                        {
                            this.Children[this.Children.Count - 1].RaisePropertyChanged("IsLast");
                        }
                    }

                    break;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        /// <summary>
        /// Gets called when the item is double-clicked.
        /// </summary>
        public virtual void ActivateItem(RoutedEventArgs e)
        {
        }

        public override string ToString()
        {
            // used for keyboard navigation
            object text = this.Text;
            return text != null ? text.ToString() : string.Empty;
        }
    }
}
