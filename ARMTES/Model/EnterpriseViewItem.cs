using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TMP.ARMTES.Model
{
    
    public class EnterpriseViewItem
    {
        public int EnterpriseId { get; set; }
        public string EnterpriseName { get; set; }
        public string EnterpriseType { get; set; }
        public List<EnterpriseViewItem> ChildEnterprises { get; set; }

        public EnterpriseViewItem()
        {
            ChildEnterprises = new List<EnterpriseViewItem>();
        }
    }
}
