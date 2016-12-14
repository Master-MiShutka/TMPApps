using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    public class ArchGroup : IArchAP
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Type_Name { get; set; }
        public string Type { get { return "GROUP"; } }
    }
}
