using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TMP.Work.Emcos.Model
{
    using Balance;
    public static class ModelConverter
    {
        public static EmcosPointWithValue PointToPointWithValue(EmcosPoint root)
        {
            var result = new EmcosPointWithValue();

            if (root.Children == null) return null;

            foreach (EmcosPoint departament in root.Children) // департаменты
            {
                var d = new EmcosPointWithValue(departament);
                d.Parent = result;

                foreach (EmcosPoint substation in departament.Children) // подстанции
                {
                    var s = new EmcosPointWithValue(substation);
                    s.Parent = d;

                    foreach (EmcosPoint group in substation.Children) // группы
                    {
                        var g = new EmcosPointWithValue(group);
                        g.Parent = s;

                        foreach (var subgroup in group.Children)
                        {
                            var p = new EmcosPointWithValue(subgroup);
                            p.Parent = g;

                            if (subgroup.Children == null)
                            {
                                g.Children.Add(p);
                                continue;
                            }

                            foreach (var section in subgroup.Children)
                            {
                                var ss = new EmcosPointWithValue(section);
                                ss.Parent = p;
                                if (section.Children != null)
                                    foreach (var fider in section.Children)
                                    {
                                        var f = new EmcosPointWithValue(fider);
                                        f.Parent = ss;
                                        if (fider.Children != null)
                                            System.Diagnostics.Debugger.Break();

                                        ss.Children.Add(f);
                                    }

                                p.Children.Add(ss);
                            }

                            g.Children.Add(p);
                        }
                        s.Children.Add(g);
                    }
                    d.Children.Add(s);
                }
                result.Children.Add(d);
            }
            return result;
        }

        public static SubstationsCollection PointToBalanceSubstations(EmcosPoint root)
        {
            var result = new SubstationsCollection(null);

            if (root.Children == null) return null;

            foreach (EmcosPoint departamentPoint in root.Children) // департаменты (РЭС)
            {
                if (departamentPoint.ElementType == ElementTypes.DEPARTAMENT && departamentPoint.TypeCode == "RES" && departamentPoint.Children != null)
                    foreach (EmcosPoint substationPoint in departamentPoint.Children) // подстанции
                    {
                        var substation = new Substation();
                        substation.Departament = departamentPoint.Name;
                        // название имеет формат вида ПС 35кВ Бакшты
                        var nameParts = substationPoint.Name.Split(' ');
                        if (nameParts.Length < 3)
                            throw new System.ArgumentException(string.Format("Неверный формат названия подстанции - '{0}'", substationPoint.Name));
                        // выделяем название
                        var name = "";
                        for (int i = 2; i < nameParts.Length; i++) name += nameParts[i];
                        substation.Name = name;
                        // выделяем напряжение
                        substation.Voltage = nameParts[1];

                        substation.Code = substationPoint.Code;
                        substation.Id = substationPoint.Id;

                        if (substationPoint.Children != null)
                            foreach (EmcosPoint groupPoint in substationPoint.Children) // группы
                            {
                                if (groupPoint.Children == null) continue;

                                IBalanceGroupItem group;
                                IBalanceItem item;
                                switch (groupPoint.TypeCode)
                                {
                                    case "AUXILIARY":
                                        group = new SubstationAuxiliary();
                                        foreach (var subgroupPoint in groupPoint.Children)
                                        {
                                            if (subgroupPoint.ElementType == ElementTypes.UNITTRANSFORMER)
                                                item = new UnitTransformer();
                                            else
                                                item = new UnitTransformerBus();
                                            item.Name = subgroupPoint.Name;
                                            item.Code = subgroupPoint.Code;
                                            item.Id = subgroupPoint.Id;
                                            item.Description = subgroupPoint.Description;
                                            item.SetSubstation(substation);
                                            group.Children.Add(item);
                                        }
                                        substation.Children.Add(group);
                                        break;

                                    case "POWERTRANSFORMERS":
                                        group = new SubstationPowerTransformers();
                                        foreach (var subgroupPoint in groupPoint.Children)
                                        {
                                            item = new PowerTransformer
                                            {
                                                Name = subgroupPoint.Name,
                                                Code = subgroupPoint.Code,
                                                Id = subgroupPoint.Id,
                                                Description = subgroupPoint.Description
                                            };
                                            item.SetSubstation(substation);
                                            group.Children.Add(item);
                                        }
                                        substation.Children.Add(group);
                                        break;

                                    case "SECTIONS":
                                        // проход по ступеням напряжения
                                        foreach (var subgroupPoint in groupPoint.Children)
                                        {
                                            SubstationSection highSection = null;
                                            // по секциям
                                            if (subgroupPoint.Children != null)
                                                foreach (var sectionPoint in subgroupPoint.Children)
                                                {
                                                    if (sectionPoint.TypeCode == "SECTIONBUS")
                                                    {
                                                        var substationSection = new SubstationSection
                                                        {
                                                            Voltage = subgroupPoint.Name,
                                                            Name = sectionPoint.Name,
                                                            Code = sectionPoint.Code,
                                                            Id = sectionPoint.Id,
                                                            Description = sectionPoint.Description
                                                        };
                                                        substationSection.SetSubstation(substation);

                                                        if (sectionPoint.Children != null)
                                                        {
                                                            foreach (var sectionChildPoint in sectionPoint.Children)
                                                            {
                                                                IBalanceItem childItem = null;
                                                                switch (sectionChildPoint.ElementType)
                                                                {
                                                                    case ElementTypes.FIDER:
                                                                        childItem = new Fider();
                                                                        break;
                                                                    case ElementTypes.POWERTRANSFORMER:
                                                                        childItem = new PowerTransformer();
                                                                        break;
                                                                    case ElementTypes.UNITTRANSFORMERBUS:
                                                                        childItem = new UnitTransformerBus();
                                                                        break;
                                                                    case ElementTypes.UNITTRANSFORMER:
                                                                        childItem = new UnitTransformer();
                                                                        break;
                                                                }
                                                                if (childItem == null)
                                                                    System.Diagnostics.Debugger.Break();

                                                                childItem.Name = sectionChildPoint.Name;
                                                                childItem.Code = sectionChildPoint.Code;
                                                                childItem.Id = sectionChildPoint.Id;
                                                                childItem.Description = sectionChildPoint.Description;
                                                                childItem.SetSubstation(substation);

                                                                substationSection.Children.Add(childItem);
                                                            }
                                                        }
                                                        substation.Children.Add(substationSection);
                                                    }
                                                    else
                                                    {
                                                        if (highSection == null)
                                                        {
                                                            highSection = new SubstationSection
                                                            {
                                                                Voltage = subgroupPoint.Name,
                                                                Name = subgroupPoint.Name,
                                                                Code = subgroupPoint.Code,
                                                                Id = subgroupPoint.Id,
                                                                Description = subgroupPoint.Description
                                                            };
                                                            highSection.SetSubstation(substation);
                                                        }
                                                        switch (sectionPoint.EcpName)
                                                        {
                                                            case "TRANSFORMER":
                                                                var pt = new PowerTransformer
                                                                {
                                                                    Id = sectionPoint.Id,
                                                                    Code = sectionPoint.Code,
                                                                    Name = sectionPoint.Name,
                                                                    Description = sectionPoint.Description
                                                                };
                                                                pt.SetSubstation(substation);
                                                                highSection.Children.Add(pt);
                                                                break;

                                                            case "LINE":
                                                                var f = new Fider
                                                                {
                                                                    Id = sectionPoint.Id,
                                                                    Code = sectionPoint.Code,
                                                                    Name = sectionPoint.Name,
                                                                    Description = sectionPoint.Description
                                                                };
                                                                f.SetSubstation(substation);
                                                                highSection.Children.Add(f);
                                                                break;
                                                        }
                                                    }
                                                }
                                            if (highSection != null)
                                                substation.Children.Add(highSection);
                                        }
                                        break;
                                }
                            }
                        result.Add(substation);
                    }
            }
            return result;
        }
    }
}