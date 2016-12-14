﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "SubstationPowerTransformers")]
    public class SubstationPowerTransformers : GroupItem
    {
        public SubstationPowerTransformers()
        {
            Title = "Трансформаторы";
            Type = ElementTypes.PowerTransformers;
            Children = new ObservableCollection<IBalansItem>();
        }
        public override IBalansItem Copy()
        {
            var s = new SubstationPowerTransformers
            {
                Id = this.Id,
                Code = this.Code,
                Title = this.Title,
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
        public override double? VvodaIn
        {
            get
            {
                var result = new Nullable<double>(0);
                if (Children != null)
                    foreach (IBalansItem item in Children)
                        if (item is PowerTransformer)
                        {
                            var bpt = item as PowerTransformer;
                            if (bpt != null && bpt.EnergyIn.HasValue)
                                result += bpt.EnergyIn;
                        }
                return result == 0 ? null : result;
            }
            set { }
        }
        public override double? VvodaOut
        {
            get
            {
                var result = new Nullable<double>(0);
                if (Children != null)
                    foreach (IBalansItem item in Children)
                        if (item is PowerTransformer)
                        {
                            var bpt = item as PowerTransformer;
                            if (bpt != null && bpt.EnergyOut.HasValue)
                                result += bpt.EnergyOut;
                        }
                return result == 0 ? null : result;
            }
            set { }
        }
        public override double? EnergyIn { get { return VvodaIn; } }
        public override double? EnergyOut { get { return VvodaOut; } }
    }
}
