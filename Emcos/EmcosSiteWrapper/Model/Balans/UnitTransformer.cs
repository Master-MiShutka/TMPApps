using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "UnitTransformer")]
    public class UnitTransformer : BalansItem, IBalansItem
    {
        public override ElementTypes Type { get { return ElementTypes.UnitTransformer; } }
        public override IBalansItem Copy()
        {
            IBalansItem obj = new UnitTransformer
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
