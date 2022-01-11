using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Подстанция
    /// </summary>
    [DataContract(Name = "Substation", Namespace = "http://tmp.work.Balance-substations.com")]
    public class Substation : SubstationSection
    {
        public Substation()
        {
            Status = DataStatus.Wait;
        }
        public override IBalanceItem Copy()
        {
            var s = new Substation
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                Departament = this.Departament,
                Voltage = this.Voltage
            };
            s.SetSubstation(this.Substation);
            foreach (IBalanceItem child in this.Children)
                s.Children.Add(child.Copy());

            return s;
        }
        public override ElementTypes ElementType => ElementTypes.SUBSTATION;
        /// <summary>
        /// Подразделение
        /// </summary>
        [DataMember()]
        public string Departament { get; set; }

    }
}
