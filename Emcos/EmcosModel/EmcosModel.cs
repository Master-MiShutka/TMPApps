using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model
{
    public interface IEmcosPoint
    {
        [DataMember()]
        decimal Id { get; set; }
        [DataMember()]
        string Name { get; set; }
        [DataMember()]
        string Code { get; set; }
        [DataMember()]
        ElementTypes Type { get; set; }
    }

    [Serializable]
    public class EmcosPointBase : PropertyChangedBase, IEmcosPoint
    {
        [XmlAttribute]
        public decimal Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Code { get; set; }
        [XmlAttribute]
        public string TypeCode { get; set; }
        [XmlAttribute]
        public string EcpName { get; set; }
        [XmlAttribute]
        public ElementTypes Type { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        public EmcosPointBase() { }
    }
    [Serializable]
    public class EmcosPointWithValue : EmcosPointBase
    {
        //public decimal ML_ID { get; set; }
        public decimal? PL_V_plus { get; set; }
        public decimal? PL_V_minus { get; set; }

        public EmcosPointWithValue() { }

        public EmcosPointWithValue(EmcosPoint point)
        {
            this.Id = point.Id;
            this.Name = point.Name;
            this.Code = point.Code;
            this.TypeCode = point.TypeCode;
            this.EcpName = point.EcpName;
            this.Type = point.Type;
        }

        public List<EmcosPointWithValue> Children { get; set; }
        [XmlIgnore]
        public int ChildCount { get { return Children != null ? Children.Count : 0; } }
        [XmlIgnore]
        public EmcosPointWithValue Parent { get; set; }
        public void Initialize()
        {
            if (this.Children == null) return;

            if (this.Children.Count == 0)
            {
                this.Children = null;
                return;
            }
            foreach (EmcosPointWithValue child in this.Children)
            {
                child.Parent = this;
                child.Initialize();
            }

        }
    }

    [Serializable]
    public class EmcosPoint : EmcosPointBase
    {
        public EmcosPoint() { }

        public EmcosPoint(EmcosGrElement gr)
        {
            this.Name = gr.Name;
            this.Code = gr.Code;
            this.TypeCode = gr.TypeCode;
        }
        public EmcosPoint(EmcosPointElement point)
        {
            this.Name = point.Name;
            this.Code = point.Code;
            this.TypeCode = point.TypeCode;
            this.EcpName = point.EcpName;
        }

        [XmlArray]
        public List<EmcosPoint> Children { get; set; }
        [XmlIgnore]
        public int ChildCount { get { return Children != null ? Children.Count : 0; } }

        [XmlIgnore]
        public EmcosPoint Parent { get; set; }

        public void Initialize()
        {
            if (this.Children == null) return;

            if (this.Children.Count == 0)
            {
                this.Children = null;
                return;
            }
            foreach (EmcosPoint child in this.Children)
            {
                child.Parent = this;
                child.Initialize();
            }
        }
    }

    public interface IEmcosElement
    {
        decimal Id { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        string TypeCode { get; set; }
        ElementTypes Type { get; set; }
    }

    public class EmcosGrElement : IEmcosElement
    {
        public decimal Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public ElementTypes Type { get; set; }
        public string ParentCode { get; set; }
        public List<EmcosPointElement> Points { get; set; }
    }

    public class EmcosPointElement : IEmcosElement
    {
        public decimal Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string EcpName { get; set; }
        public ElementTypes Type { get; set; }
    }

    public enum ElementTypes
    {
        GROUP,
        [Description("Подразделение")]
        DEPARTAMENT,
        [Description("Подстанция")]
        SUBSTATION,
        [Description("Ступень напряжения")]
        VOLTAGE,
        [Description("Секция шин")]
        SECTION,
        [Description("Трансформатор")]
        POWERTRANSFORMER,
        [Description("Трансформатор собственных нужд с секций")]
        UNITTRANSFORMER,
        [Description("Трансформатор собственных нужд с шин")]
        UNITTRANSFORMERBUS,
        [Description("Фидер")]
        FIDER,
        [Description("Трансформаторы")]
        POWERTRANSFORMERS,
        [Description("Собственные нужды")]
        AUXILIARY
    }
}
