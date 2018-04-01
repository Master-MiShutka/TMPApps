using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "PowerTransformer")]
    public class PowerTransformer : BalansItem
    {
        public override ElementTypes Type { get { return ElementTypes.POWERTRANSFORMER; } }
        public override IBalansItem Copy()
        {
            IBalansItem obj = new PowerTransformer
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description
            };
            obj.SetSubstation(this.Substation);
            return obj;
        }
    }
}
