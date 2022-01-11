using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Группа силовых трансформаторов
    /// </summary>
    [DataContract(Name = "SubstationPowerTransformers")]
    public class SubstationPowerTransformers : BalanceGroupItem
    {
        public SubstationPowerTransformers()
        {
            Name = "Трансформаторы";
        }
        public override IBalanceItem Copy()
        {
            var s = new SubstationPowerTransformers
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
        public override ElementTypes ElementType => ElementTypes.POWERTRANSFORMERS;
    }
}
