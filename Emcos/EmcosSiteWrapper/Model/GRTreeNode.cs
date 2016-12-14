using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    using TMP.Work.Emcos.Controls.VTreeView;

    public class GRTreeNode : TreeNode
    {
        public string Code { get; set; }
        public int Type_Id { get; set; }
        public string Type_Name { get; set; }
        public string Type_Code { get; set; }
        public byte HasChilds { get; set; }
        public string Type { get; set; }
        public string Parent { get; set; }

        public string Ecp_Name { get; set; }

        public bool IsIniatialyExpanded { get; set; }

        public GRTreeNode()
        {
            Name = "<нет названия>";
            HasChilds = 0;
            IsIniatialyExpanded = false;
        }

        public override bool HasChildren { get { return HasChilds == 1; } }
    }
}
