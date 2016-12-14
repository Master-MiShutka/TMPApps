using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    using Balans;
    public interface IBalansSession
    {
        string FileName { get; set; }
        DateTime LastModifiedDate { get; set; }
        long FileSize { get; set; }
        bool IsLoaded { get; set; }
        string Title { get; }
        DatePeriod Period { get; set; }
        IList<Substation> Substations { get; set; }
    }
}
