using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Export
{
    public struct ExportInfo
    {
        public string Title;
        public DateTime StartDate;
        public DateTime EndDate;
        public ICollection<Model.Balance.Substation> Substations;
    }
}
