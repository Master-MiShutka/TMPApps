using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Фидер
    /// </summary>
    [DataContract(Name = "Fider")]
    public class Fider : BalanceItem
    {
        public override ElementTypes ElementType { get { return ElementTypes.FIDER; } }
        public override IBalanceItem Copy()
        {
            IBalanceItem obj = new Fider
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
