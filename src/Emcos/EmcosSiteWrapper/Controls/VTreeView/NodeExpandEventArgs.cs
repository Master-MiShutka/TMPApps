using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Controls.VTreeView
{
    public class NodeExpandEventArgs : EventArgs
    {
        public NodeExpandEventArgs(ITreeNode node)
        {
            Node = node;
        }
        public NodeExpandEventArgs(ITreeNode node, Object obj)
        {
            Node = node;
            Obj = obj;
        }
        public ITreeNode Node { get; set; }
        public Object Obj { get; set; }
    }
}
