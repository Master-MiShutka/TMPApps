using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Трфнсформатор собственных нужд, подключенный к шинам
    /// </summary>
    [DataContract(Name = "UnitTransformerBus")]
    public class UnitTransformerBus : BalanceItem
    {
        public override ElementTypes ElementType { get { return ElementTypes.UNITTRANSFORMERBUS; } }
        public override IBalanceItem Copy()
        {
            IBalanceItem obj = new UnitTransformerBus
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
