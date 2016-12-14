using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    public class ArchPoint : IArchAP
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CODE { get; set; }
        public string Ecp_Name { get; set; }
        public string Type_Name { get; set; }
        public string Type { get { return "POINT"; } }
    }
}
