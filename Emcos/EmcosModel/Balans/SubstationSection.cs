using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Секция шин
    /// </summary>
    [DataContract(Name = "SubstationSection")]
    public class SubstationSection : BalanceGroupItem
    {
        public SubstationSection() : base()
        {
        }
        public override IBalanceItem Copy()
        {
            var s = new SubstationSection
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                Voltage = this.Voltage
            };
            s.SetSubstation(this.Substation);
            foreach (IBalanceItem child in this.Children)
                s.Children.Add(child.Copy());

            return s;
        }

        public override ElementTypes ElementType => ElementTypes.SECTION;
        /// <summary>
        /// Напряжение
        /// </summary>
        [DataMember()]
        public String Voltage { get; set; }
        /// <summary>
        /// Это напряжение 6-10кВ?
        /// </summary>
        public bool IsLowVoltage
        {
            get
            {
                return (Voltage == "10кВ" || Voltage == "6кВ");
            }
        }
    }
}
