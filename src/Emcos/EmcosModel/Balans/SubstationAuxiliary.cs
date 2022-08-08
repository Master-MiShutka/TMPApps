using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Группа трансформаторов собственных нужд
    /// </summary>
    [DataContract(Name = "SubstationAuxiliary")]
    public class SubstationAuxiliary : BalanceGroupItem
    {
        public SubstationAuxiliary()
        {
            Name = "Собственные нужды";
        }
        public override IBalanceItem Copy()
        {
            var s = new SubstationAuxiliary
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
            };
            s.SetSubstation(this.Substation);
            foreach (IBalanceItem child in this.Children)
                s.Children.Add(child.Copy());

            return s;
        }
        public override ElementTypes ElementType => ElementTypes.AUXILIARY;
    }
}
