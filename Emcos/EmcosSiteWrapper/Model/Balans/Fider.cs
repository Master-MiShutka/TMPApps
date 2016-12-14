using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "Fider")]
    public class Fider : BalansItem, IBalansItem
    {
        public override ElementTypes Type { get { return ElementTypes.Fider; } }
        public override IBalansItem Copy()
        {
            IBalansItem obj = new Fider
            {
                Id = this.Id,
                Code = this.Code,
                Title = this.Title,
                Description = this.Description
            };
            obj.SetSubstation(this.Substation);
            return obj;
        }
    }
}
