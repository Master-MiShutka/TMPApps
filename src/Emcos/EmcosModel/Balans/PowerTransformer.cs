using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Силовой трансформатор
    /// </summary>
    [DataContract(Name = "PowerTransformer")]
    public class PowerTransformer : BalanceItem
    {
        public override ElementTypes ElementType { get { return ElementTypes.POWERTRANSFORMER; } }
        public override IBalanceItem Copy()
        {
            IBalanceItem obj = new PowerTransformer
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
