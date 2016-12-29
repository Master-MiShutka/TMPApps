using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "SubstationSection")]
    public class SubstationSection : GroupItem
    {
        public SubstationSection()
        {
            Type = ElementTypes.SECTION;
            Children = new ObservableCollection<IBalansItem>();
        }
        public override IBalansItem Copy()
        {
            var s = new SubstationSection
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                Type = this.Type,
                Voltage = this.Voltage,
                Children = new ObservableCollection<IBalansItem>()
            };
            s.SetSubstation(this.Substation);
            foreach (IBalansItem child in this.Children)
                s.Children.Add(child.Copy());
            s.UpdateChildren();
            return s;
        }
        [DataMember()]
        public String Voltage { get; set; }
        public bool IsLowVoltage
        {
            get
            {
                return (Voltage == "10кВ" || Voltage == "6кВ");
            }
        }
        public override double? Unbalance { get { return EnergyIn - EnergyOut == 0.0 ? null : EnergyIn - EnergyOut; } }
        public override double? PercentageOfUnbalance { get { return EnergyIn == 0.0 ? null : 100.0 * Unbalance / EnergyIn; } }
        public override double? VvodaIn
        {
            get
            {
                var result = new Nullable<double>(0);
                if (Children != null)
                {
                    foreach (IBalansItem item in Children)
                        if (item is PowerTransformer)
                        {
                            var bpt = item as PowerTransformer;
                            if (bpt != null)
                            {
                                var value = bpt.EnergyIn;
                                if (value.HasValue)
                                    result += value;
                            }
                        }
                }
                else
                    return null;
                return result;
            }

            set { }
        }
        public override double? VvodaOut
        {
            get
            {
                var result = new Nullable<double>(0);
                if (Children != null)
                {
                    foreach (IBalansItem item in Children)
                        if (item is PowerTransformer)
                        {
                            var bpt = item as PowerTransformer;
                            if (bpt != null)
                            {
                                var value = bpt.EnergyOut;
                                if (value.HasValue)
                                    result += value;
                            }
                        }
                }
                else
                    return null;
                return result;
            }

            set { }
        }
        public override double? FideraIn
        {
            get
            {
                if (IsLowVoltage == false)
                    return null;
                var result = new Nullable<double>(0);
                if (Children != null)
                {
                    foreach (IBalansItem item in Children)
                        if (item is Fider)
                        {
                            var bf = item as Fider;
                            if (bf != null)
                            {
                                var value = bf.EnergyIn;
                                if (value.HasValue)
                                    result += value;
                            }
                        }
                }
                else
                    return null;
                return result;
            }

            set { }
        }
        public override double? FideraOut
        {
            get
            {
                if (IsLowVoltage == false)
                    return null;
                var result = new Nullable<double>(0);
                if (Children != null)
                {
                    foreach (IBalansItem item in Children)
                        if (item is Fider)
                        {
                            var bf = item as Fider;
                            if (bf != null)
                            {
                                var value = bf.EnergyOut;
                                if (value.HasValue)
                                    result += value;
                            }
                        }
                }
                else
                    return null;
                return result;
            }
            set { }
        }
        public override double? Tsn
        {
            get
            {
                if (IsLowVoltage == false)
                    return null;
                var result = new Nullable<double>(0);
                if (Children != null)
                {
                    foreach (IBalansItem item in Children)
                        if (item is UnitTransformerBus)
                        {
                            var utb = item as UnitTransformerBus;
                            if (utb != null && utb.EnergyIn.HasValue)
                                result += utb.EnergyIn.Value;
                        }
                }
                else
                    return null;
                return result;
            }

            set { }
        }

        public bool ExcessUnbalance
        {
            get
            {
                return Unbalance.HasValue ? Math.Abs(Unbalance.Value) > 3.0 : false;
            }
        }
    }
}
