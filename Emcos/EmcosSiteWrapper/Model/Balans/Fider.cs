using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "Fider")]
    public class Fider : BalansItem
    {
        public override ElementTypes Type { get { return ElementTypes.FIDER; } }
        public override IBalansItem Copy()
        {
            IBalansItem obj = new Fider
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
