using System.Collections.Generic;
using System.ComponentModel;

namespace TMP.Work.Emcos.Controls.VTreeView
{
    public abstract class TreeNode : INotifyPropertyChanged, ITreeNode
    {
        public TreeNode() { Level = 0; }
        public int Level { get; set; }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        public ICollection<ITreeNode> Children { get; set; }
        public virtual bool HasChildren { get { return (Children.Count > 0); } }
        private TreeNodeState _state = TreeNodeState.Ready;
        public TreeNodeState State
        {
            get { return _state; }
            set
            {
                if (value != _state)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }
        public virtual ITreeNode ParentNode { get; set; }
        public void SetParent(ITreeNode parent)
        {
            ParentNode = parent;
        }
        public virtual void UpdateChildren()
        {
            OnPropertyChanged("HasChildren");
        }

        public string Id { get; set; }
        public string Name { get; set; }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    public enum TreeNodeState
    {
        Ready,
        PrepareChildren,
        ChildrenPrepared
    }
}
