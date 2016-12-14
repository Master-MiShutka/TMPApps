using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    public class ArchAP
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IArchAP AP { get; set; }
    }
}
