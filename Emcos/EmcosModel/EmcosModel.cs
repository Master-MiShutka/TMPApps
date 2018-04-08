using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace TMP.Work.Emcos.Model
{
    public interface IEmcosPoint
    {
        [DataMember]
        int Id { get; set; }
        [DataMember]
        string Name { get; set; }
        [DataMember]
        string Code { get; set; }
        [DataMember]
        string TypeCode { get; set; }

        ElementTypes ElementType { get; }
        [DataMember]
        string EcpName { get; set; }
        [DataMember]
        string Description { get; set; }
    }

    public interface IHierarchicalEmcosPoint : IEmcosPoint
    {
        [DataMember]
        string ParentTypeCode { get; set; }
        [DataMember]
        bool IsGroup { get; set; }

        [DataMember]
        ObservableCollection<IHierarchicalEmcosPoint> Children { get; set; }
    }

    [Serializable]
    [DataContract]
    public class EmcosPointBase : PropertyChangedBase, IEmcosPoint
    {
        [XmlAttribute]
        [DataMember]
        public int Id { get; set; }

        [XmlAttribute]
        [DataMember]
        [Magic]
        public string Name { get; set; }
        [XmlAttribute]
        [DataMember]
        public string Code { get; set; }
        [XmlAttribute]
        [DataMember]
        public string TypeCode { get; set; }
        [XmlAttribute]
        [DataMember]
        public string EcpName { get; set; }
        [XmlAttribute]
        [DataMember]
        public ElementTypes ElementType { get; set; }

        [XmlAttribute]
        [DataMember]
        [Magic]
        public string Description { get; set; }

        public EmcosPointBase() { }

        public override int GetHashCode()
        {
            return (ElementType.GetHashCode() % 0x8000) | (int)((uint)Id.GetHashCode() & 0xFFFF0000);
        }

        public override string ToString()
        {
            return String.Format("Id:[{0}], Code:[{1}], TypeCode:[{2}], Type:[[3}]", Id, Code, TypeCode, ElementType);
        }
    }
    [Serializable]
    public class EmcosPointWithValue : EmcosPointBase
    {
        //public decimal ML_ID { get; set; }
        public decimal? PL_V_plus { get; set; }
        public decimal? PL_V_minus { get; set; }

        public EmcosPointWithValue() { }

        public EmcosPointWithValue(IHierarchicalEmcosPoint point)
        {
            this.Id = point.Id;
            this.Name = point.Name;
            this.Code = point.Code;
            this.TypeCode = point.TypeCode;
            this.EcpName = point.EcpName;
            this.ElementType = point.ElementType;
        }
        [Magic]
        public ObservableCollection<EmcosPointWithValue> Children { get; set; }
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
    [DataContract]
    public class EmcosPoint : EmcosPointBase, IHierarchicalEmcosPoint
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

        [XmlAttribute]
        [DataMember]
        public string ParentTypeCode { get; set; }
        [XmlAttribute]
        [DataMember]
        public bool IsGroup { get; set; }

        [XmlArray]
        [DataMember]
        [Magic]
        public ObservableCollection<IHierarchicalEmcosPoint> Children { get; set; }


        [XmlIgnore]
        [IgnoreDataMember]
        public int ChildCount { get { return Children != null ? Children.Count : 0; } }

        [XmlIgnore]
        [IgnoreDataMember]
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
        int Id { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        string TypeCode { get; set; }
        ElementTypes Type { get; set; }
    }

    public class EmcosGrElement : IEmcosElement
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public ElementTypes Type { get; set; }
        public string ParentCode { get; set; }
        public List<EmcosPointElement> Points { get; set; }
    }

    public class EmcosPointElement : IEmcosElement
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string EcpName { get; set; }
        public ElementTypes Type { get; set; }
    }

    public enum ElementTypes
    {
        GROUP,
        POINT,
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
