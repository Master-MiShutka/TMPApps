using System.Collections.Generic;

namespace TMP.Work.Emcos.Model
{
    public interface IParam
    {
        string Id { get; set; }
        string Name { get; set; }
        string Type { get;  }
        bool HasChildren { get; }
        ICollection<IParam> Children { get; set; }
    }
}
