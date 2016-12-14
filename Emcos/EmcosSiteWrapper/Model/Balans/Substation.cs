using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace TMP.Work.Emcos.Model.Balans
{
    [DataContract(Name = "Substation", Namespace = "http://tmp.work.balans-substations.com")]
    public class Substation : SubstationSection
    {
        private double? _maximumAllowableUnbalance = null;
        public Substation()
        {
            Type = ElementTypes.Substation;
            Children = new ObservableCollection<IBalansItem>();

            Status = DataStatus.Wait;
        }
        public override IBalansItem Copy()
        {
            var s = new Substation
            {
                Id = this.Id,
                Code = this.Code,
                Title = this.Title,
                Description = this.Description,
                Type = this.Type,
                Departament = this.Departament,
                Voltage = this.Voltage,
                //Dates = this.Dates,
                Children = new ObservableCollection<IBalansItem>(),
            };
            s.SetSubstation(this.Substation);
            foreach (IBalansItem child in this.Children)
                s.Children.Add(child.Copy());
            s.UpdateChildren();
            return s;
        }

        [DataMember()]
        public string Departament { get; set; }
        public override double? VvodaIn
        {
            get
            {
                var result = new Nullable<double>(0.0);
                if (Children != null)
                {
                    foreach (IBalansItem item in Children)
                        if (item is SubstationSection)
                        {
                            var bss = item as SubstationSection;
                            if (bss != null && bss.IsLowVoltage)
                            {
                                var value = bss.VvodaIn;
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
                var result = new Nullable<double>(0.0);
                if (Children != null)
                {
                    foreach (IBalansItem item in Children)
                        if (item is SubstationSection)
                        {
                            var bss = item as SubstationSection;
                            if (bss != null && bss.IsLowVoltage)
                            {
                                var value = bss.VvodaOut;
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
                var result = new Nullable<double>(0.0);
                if (Children != null)
                    foreach (IBalansItem item in Children)
                        if (item is SubstationSection)
                        {
                            var bss = item as SubstationSection;
                            if (bss != null && bss.IsLowVoltage)
                            {
                                var value = bss.FideraIn;
                                if (value.HasValue)
                                    result += value;
                            }
                        }
                return result == 0.0 ? null : result;
            }

            set { }
        }
        public override double? FideraOut
        {
            get
            {
                var result = new Nullable<double>(0.0);
                if (Children != null)
                    foreach (IBalansItem item in Children)
                        if (item is SubstationSection)
                        {
                            var bss = item as SubstationSection;
                            if (bss != null && bss.IsLowVoltage)
                            {
                                var value = bss.FideraOut;
                                if (value.HasValue)
                                    result += value;
                            }
                        }
                return result == 0.0 ? null : result;
            }
            set { }
        }
        public override double? Tsn
        {
            get
            {
                var result = new Nullable<double>(0.0);
                if (Children != null)
                    foreach (IBalansItem item in Children)
                        if (item is SubstationSection)
                        {
                            var bss = item as SubstationSection;
                            if (bss != null && bss.IsLowVoltage)
                            {
                                var value = bss.Tsn;
                                if (value.HasValue)
                                    result += value;
                            }
                        }
                return result == 0.0 ? null : result;
            }
        }
        public bool HasData
        {
            get
            {
                if (Children == null || Children.Count == 0) return true;

                return Children
                    .All((item) =>
                    {
                        var bss = item as SubstationSection;
                        if (bss != null && bss.IsLowVoltage && (bss.FideraIn.HasValue || bss.FideraOut.HasValue))
                            return true;
                        else
                            return false;
                    });
            }
        }

        public override double? MaximumAllowableUnbalance
        {
            get
            {
                if (_maximumAllowableUnbalance != null)
                    return _maximumAllowableUnbalance;
                // погрешность учёта
                double δсч = 0.5d;
                // погрешность тр-ра тока
                double δтт = 0.5d;
                // погрешность тр-ра напряжения
                double δтн = 0.5d;
                // суммарная относительная погрешность
                double teta = 1.1 * Math.Sqrt(δсч* δсч + δтт * δтт + δтн * δтн);

                var powertrans = Items
                    .Where(i => i.Type == ElementTypes.PowerTransformer)
                    .Select(i => new { Name = i.Title, In = i.EnergyIn, Out = i.EnergyOut }).ToList();
                var fiders = Items
                    .Where(i => i.Type == ElementTypes.Fider)
                    .Select(i => new { Name = i.Title, In = i.EnergyIn, Out = i.EnergyOut }).ToList();
                var tsn = Items
                    .Where(i => (i.Type == ElementTypes.UnitTransformer || i.Type == ElementTypes.UnitTransformerBus))
                    .Select(i => new { Name = i.Title, In = i.EnergyIn, Out = i.EnergyOut }).ToList();                

                double energyInFiders = fiders
                    .Sum(i => i.In.HasValue ? i.In.Value / 1000d: 0);
                double energyInTsn = tsn
                    .Sum(i => i.Out.HasValue ? i.Out.Value / 1000d : 0);
                double energyInPower = powertrans
                    .Sum(i => i.In.HasValue ? i.In.Value / 1000d : 0);
                double summEnergyIn = energyInFiders + energyInTsn + energyInPower;

                double energySqrInPower = powertrans
                    .Sum(i => Math.Pow(i.In.HasValue ? i.In.Value / 1000d : 0, 2));
                double energySqrInFiders = fiders
                    .Sum(i => Math.Pow(i.In.HasValue ? i.In.Value / 1000d : 0, 2));
                double energySqrInTsn = tsn
                    .Sum(i => Math.Pow(i.Out.HasValue ? i.Out.Value / 1000d : 0, 2));
                double summSqrEnergyIn = energySqrInPower + energySqrInFiders + energySqrInTsn;

                return Math.Sqrt(Math.Pow(teta, 2) * summSqrEnergyIn / Math.Pow(summEnergyIn, 2));
            }
            set
            {
                _maximumAllowableUnbalance = value;
                RaisePropertyChanged("MaximumAllowableUnbalance");
            }
        }
    }
}
