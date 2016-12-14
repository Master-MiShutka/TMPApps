using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;

namespace TMP.Work.Emcos.Model
{
    [Serializable]
    public class EmcosPointBase : PropertyChangedBase
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
        public string EcpCode { get; set; }
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
            this.EcpCode = point.EcpCode;
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
            this.EcpCode = point.EcpCode;
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

    public class EmcosGrElement
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string ParentCode { get; set; }
        public List<EmcosPointElement> Points { get; set; }
    }

    public class EmcosPointElement
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string EcpCode { get; set; }
    }

    public enum ElementTypes
    {
        Group,
        [Description("Подразделение")]
        Departament,
        [Description("Подстанция")]
        Substation,
        [Description("Ступень напряжения")]
        Voltage,
        [Description("Секция шин")]
        Section,
        [Description("Трансформатор")]
        PowerTransformer,
        [Description("Трансформатор собственных нужд с секций")]
        UnitTransformer,
        [Description("Трансформатор собственных нужд с шин")]
        UnitTransformerBus,
        [Description("Фидер")]
        Fider,
        [Description("Трансформаторы")]
        PowerTransformers,
        [Description("Собственные нужды")]
        Auxiliary
    }
}
