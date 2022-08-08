using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TMP.Work.Emcos.Controls.VTreeView
{
    public interface ITreeNode : INotifyPropertyChanged
    {
        int Level { get; set; }
        string Name { get; set; }
        string Id { get; set; }

        ICollection<ITreeNode> Children { get; set; }

        bool HasChildren { get; }
        bool IsExpanded { get; set; }
        void UpdateChildren();
        ITreeNode ParentNode { get; }
        void SetParent(ITreeNode parent);
        TreeNodeState State { get; set; }
    }
}
