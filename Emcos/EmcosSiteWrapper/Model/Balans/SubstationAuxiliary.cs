using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "SubstationAuxiliary")]
    public class SubstationAuxiliary : GroupItem
    {
        public SubstationAuxiliary()
        {
            Name = "Собственные нужды";
            Type = ElementTypes.AUXILIARY;
            Children = new ObservableCollection<IBalansItem>();
        }
        public override IBalansItem Copy()
        {
            var s = new SubstationAuxiliary
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                Type = this.Type,
                Children = new ObservableCollection<IBalansItem>()
            };
            s.SetSubstation(this.Substation);
            foreach (IBalansItem child in this.Children)
                s.Children.Add(child.Copy());
            s.UpdateChildren();
            return s;
        }
        public override double? Tsn
        {
            get
            {
                var result = new Nullable<double>(0);
                if (Children != null)
                    foreach (IBalansItem item in Children)
                        if (item is UnitTransformer)
                        {
                            var but = item as UnitTransformer;
                            if (but != null && but.EnergyIn.HasValue)
                                if (but.Type == ElementTypes.UNITTRANSFORMERBUS)
                                    result += but.EnergyIn;
                        }
                return result == 0 ? null : result;
            }
            set { }
        }
        public override double? EnergyIn { get { return Eplus; } }
        public override double? EnergyOut { get { return Eminus; } }
    }
}
