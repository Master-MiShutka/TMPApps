namespace TMP.Shared.Tree
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract class to use it as model in TreeView control.
    /// Support filter by header
    /// </summary>
    public abstract class TreeNode : PropertyChangedBase, ITreeNode
    {
        private ITreeNode parent;
        private string header;
        private IList<ITreeNode> children = new List<ITreeNode>();
        private bool isExpanded;

        // default value is true
        private bool isMatch = true;

        // default value is true
        private bool isExpandable = true;

        /// <summary>
        /// Element level
        /// </summary>
        public int Level => this.parent == null ? 0 : this.parent.Level + 1;

        /// <summary>
        /// Parent element
        /// </summary>
        public ITreeNode Parent { get => this.parent; set => this.SetProperty(ref this.parent, value); }

        /// <summary>
        /// Header
        /// </summary>
        public string Header { get => this.header; set => this.SetProperty(ref this.header, value); }

        /// <summary>
        /// True is header is empty
        /// </summary>
        public bool IsHeaderEmpty => string.IsNullOrWhiteSpace(this.Header);

        /// <summary>
        /// Children elements
        /// </summary>
        public IList<ITreeNode> Children
        {
            get => this.children;
            set
            {
                if (this.SetProperty(ref this.children, value))
                {
                    this.RaisePropertyChanged(nameof(this.HasChildren));
                    this.OnChildrenChanged();
                }
            }
        }

        /// <summary>
        /// True is has children
        /// </summary>
        public bool HasChildren => this.children != null && this.children.Count != 0;

        /// <summary>
        /// True is node can be expanded
        /// </summary>
        public bool IsExpandable
        {
            get => this.isExpandable;
            set => this.SetProperty(ref this.isExpandable, value);
        }

        /// <summary>
        /// True if node should be expanded
        /// </summary>
        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (this.SetProperty(ref this.isExpanded, value))
                {
                    if (this.isExpanded)
                    {
                        foreach (ITreeNode child in this.Children)
                        {
                            child.IsMatch = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// True is node should be displayed
        /// </summary>
        public bool IsMatch
        {
            get => this.isMatch;
            set => this.SetProperty(ref this.isMatch, value);
        }

        /// <summary>
        /// Callback on children collection changed
        /// </summary>
        protected abstract void OnChildrenChanged();

        public TreeNode() { }

        public TreeNode(ITreeNode parent, string header)
        {
            this.parent = parent;
            this.header = header;
        }

        /// <summary>
        /// Return True is <param name="criteria" /> matched
        /// </summary>
        /// <remarks>
        /// By default checking <see cref="Header" /> contains <param name="criteria" />
        /// </remarks>
        /// <param name="criteria">Searching string</param>
        /// <returns>True is matched</returns>
        protected virtual bool IsCriteriaMatched(string criteria)
        {
            return string.IsNullOrEmpty(criteria) || this.Header.Contains(criteria, StringComparison.CurrentCultureIgnoreCase);
        }

        public void ApplyCriteria(string criteria, Stack<ITreeNode> ancestors)
        {
            if (ancestors == null)
            {
                return;
            }

            if (this.IsCriteriaMatched(criteria))
            {
                this.IsMatch = true;
                foreach (ITreeNode ancestor in ancestors)
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
            foreach (ITreeNode child in this.Children)
            {
                child.ApplyCriteria(criteria, ancestors);
            }

            ancestors.Pop();
        }
    }
}
