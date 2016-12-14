using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Emcos.Test.Model
{
    using TMP.Emcos.Test.Controls.VTreeView;
    public abstract class ParamTreeNode : TreeNode
    {        
        public virtual string TYPE { get;}
        public ParamTreeNode()
        {

        }
        public override bool HasChildren
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return NAME;
            }
        }
    }
}
