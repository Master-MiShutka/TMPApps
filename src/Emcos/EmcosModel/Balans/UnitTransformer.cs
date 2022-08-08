using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Трансформатор собственных нужд
    /// </summary>
    [DataContract(Name = "UnitTransformer")]
    public class UnitTransformer : BalanceItem
    {
        public override ElementTypes ElementType { get { return ElementTypes.UNITTRANSFORMER; } }
        public override IBalanceItem Copy()
        {
            IBalanceItem obj = new UnitTransformer
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
