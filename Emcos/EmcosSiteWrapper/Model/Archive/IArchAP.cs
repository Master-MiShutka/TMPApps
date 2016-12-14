using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    public interface IArchAP
    {
        string Id { get; set; }
        string Name { get; set; }
        string Type { get; }
        string Type_Name { get; }
    }
}
