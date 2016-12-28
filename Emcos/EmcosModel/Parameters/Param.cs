using System.Collections.Generic;

namespace TMP.Work.Emcos.Model
{
    public abstract class Param : IParam
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual string Type { get;}
        public Param()
        {

        }
        public virtual bool HasChildren { get { return Children == null ? false : Children.Count > 0 ? true : false ;} }

        public ICollection<IParam> Children { get; set; }
    }
}
