using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TMP.Work.Emcos.Model
{
    using TMP.Shared;

    /// <summary>
    /// Базовый интерфейс, описывающий точку из Emcos
    /// </summary>
    public interface IEmcosPoint
    {
        int Id { get; set; }
        string Name { get; set; }
        string Code { get; set; }
        string TypeCode { get; set; }
        ElementTypes ElementType { get; }
        string TypeName { get; }
        string EcpName { get; set; }
        string Description { get; set; }
    }
    /// <summary>
    /// Интерфейс, описывающий иерархическую структуру точек из Emcos
    /// </summary>
    public interface IHierarchicalEmcosPoint : IEmcosPoint, IProgress, ICloneable, INotifyPropertyChanged
    {
        IHierarchicalEmcosPoint Parent { get; set; }
        string ParentTypeCode { get; }
        bool IsGroup { get; }
        HierarchicalEmcosPointCollection Children { get; set; }
        bool HasChildren { get; }
        int ChildrenCount { get; }

        object Tag { get; set; }
    }
    /// <summary>
    /// Базовый класс точки из Emcos
    /// </summary>
    [Serializable]
    [DataContract]
    [Magic]
    public class EmcosPointBase : PropertyChangedBase, IEmcosPoint
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [XmlAttribute]
        [DataMember(IsRequired = true)]
        public int Id { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        [XmlAttribute]
        [DataMember(IsRequired = true)]
        public string Name { get; set; }
        /// <summary>
        /// Код
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public string Code { get; set; }
        /// <summary>
        /// Код типа
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public string TypeCode { get; set; }
        /// <summary>
        /// Название Ecp
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public string EcpName { get; set; }
        /// <summary>
        /// Тип элемента
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public virtual ElementTypes ElementType { get; set; }

        /// <summary>
        /// Возвращает описание типа <see cref="ElementTypes"/> из <see cref="DescriptionAttribute"/>
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public string TypeName
        {
            get
            {
                var fieldInfo = ElementType.GetType().GetField(ElementType.ToString());
                if (fieldInfo == null)
                    System.Diagnostics.Debugger.Break();
                var attribArray = fieldInfo.GetCustomAttributes(false);
                if (attribArray == null)
                    System.Diagnostics.Debugger.Break();
                var attrib = attribArray[0] as DescriptionAttribute;
                if (attrib == null)
                    System.Diagnostics.Debugger.Break();
                return attrib.Description;
            }
        }
        /// <summary>
        /// Описание
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Отмечена ли точка
        /// </summary>
        [DataMember]
        [Magic]
        public bool IsChecked { get; set; }

        public EmcosPointBase() { }

        public override int GetHashCode()
        {
            return (ElementType.GetHashCode() % 0x8000) | (int)((uint)Id.GetHashCode() & 0xFFFF0000);
        }

        public override string ToString()
        {
            return String.Format("Id:[{0}], Code:[{1}], TypeCode:[{2}], Type:[{3}]", Id, Code, TypeCode, ElementType);
        }

        public override bool Equals(object obj)
        {
            IEmcosPoint point = obj as IEmcosPoint;
            if (point == null) return false;

            return this.Id == point.Id && this.Code == point.Code && this.Name == point.Name;
        }
    }
    /// <summary>
    /// Базовый класс точки из Emcos со значеним энергии прямого и обратного направления
    /// </summary>
    [Serializable]
    public class EmcosPointWithValue : EmcosPointBase
    {
        //public decimal ML_ID { get; set; }
        /// <summary>
        /// Значение энергии прямого направления
        /// </summary>
        public decimal? PL_V_plus { get; set; }
        /// <summary>
        /// Значение энергии обратного направления
        /// </summary>
        public decimal? PL_V_minus { get; set; }

        public EmcosPointWithValue()
        {
            Children = new EmcosPointWithValueCollection(this);
        }
        public EmcosPointWithValue(IHierarchicalEmcosPoint point) : this()
        {
            this.Id = point.Id;
            this.Name = point.Name;
            this.Code = point.Code;
            this.TypeCode = point.TypeCode;
            this.EcpName = point.EcpName;
            this.ElementType = point.ElementType;
        }
        /// <summary>
        /// Коллекция дочерних элементов
        /// </summary>
        [Magic]
        public EmcosPointWithValueCollection Children { get; set; }
        /// <summary>
        /// Имеются ли дочерние элементы
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public bool HasChildren => ChildrenCount > 0;
        /// <summary>
        /// Количество дочерних элементов
        /// </summary>
        [XmlIgnore]
        public int ChildrenCount { get { return Children != null ? Children.Count : 0; } }
        /// <summary>
        /// Ссылка на родителя
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public EmcosPointWithValue Parent { get; set; }
    }

    /// <summary>
    /// Точка данных из Emcos
    /// </summary>
    [Serializable]
    [DataContract]
    [Magic]
    public class EmcosPoint : EmcosPointBase, IHierarchicalEmcosPoint, ITreeModel
    {
        HierarchicalEmcosPointCollection _children;

        public EmcosPoint()
        {
            Children = new HierarchicalEmcosPointCollection(this);
        }

        public EmcosPoint(string name, IEnumerable<IHierarchicalEmcosPoint> children) : this()
        {
            Name = name;
            foreach (IHierarchicalEmcosPoint child in children)
                Children.Add(child);
        }

        public EmcosPoint(IEnumerable<IHierarchicalEmcosPoint> children) : this()
        {
            if (children != null)
            {
                foreach (var child in children)
                    Children.Add(child);
            }
        }

        public EmcosPoint(HierarchicalEmcosPointCollection children) : this()
        {
            if (children != null)
            {
                foreach (var child in children)
                    Children.Add(child);
            }
        }

        public EmcosPoint(EmcosGrElement gr) : this()
        {
            this.Name = gr.Name;
            this.Code = gr.Code;
            this.TypeCode = gr.TypeCode;
        }

        public EmcosPoint(EmcosPointElement point) : this()
        {
            this.Name = point.Name;
            this.Code = point.Code;
            this.TypeCode = point.TypeCode;
            this.EcpName = point.EcpName;
        }

        public object Clone()
        {
            EmcosPoint clone = new EmcosPoint()
            {
                Id = this.Id,
                Name = this.Name,
                Code = this.Code,
                TypeCode = this.TypeCode,
                EcpName = this.EcpName,
                ElementType = this.ElementType,
                Description = this.Description,
                IsChecked = this.IsChecked
            };
            foreach (IHierarchicalEmcosPoint point in Children)
                clone.Children.Add((IHierarchicalEmcosPoint)point.Clone());

            return clone;
        }

        /// <summary>
        /// Ссылка на родителя
        /// </summary>
        [IgnoreDataMember]
        [XmlIgnore]
        public IHierarchicalEmcosPoint Parent { get; set; }

        /// <summary>
        /// Код типа родителя
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public string ParentTypeCode => Parent != null ? Parent.TypeCode : string.Empty;
        /// <summary>
        /// Это группа
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public bool IsGroup => HasChildren;
        /// <summary>
        /// Коллекция дочерних элементов
        /// </summary>
        [XmlArray]
        [DataMember]
        public HierarchicalEmcosPointCollection Children
        {
            get
            {
                return _children;
            }
            set
            {
                INotifyCollectionChanged collection = value as INotifyCollectionChanged;

                if (collection == null)
                    throw new InvalidCastException("Collection must be implement INotifyCollectionChanged");

                // отписка
                if (_children != null)
                    (_children as INotifyCollectionChanged).CollectionChanged -= ChildrenCollectionChanged;
                SetProperty(ref _children, value, "Children");
                // подписка
                if (_children != null)
                    (_children as INotifyCollectionChanged).CollectionChanged += ChildrenCollectionChanged;
            }
        }
        /// <summary>
        /// Имеются ли дочерние элементы
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public bool HasChildren => ChildrenCount > 0;

        /// <summary>
        /// Количество дочерних элементов
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public int ChildrenCount { get { return Children != null ? Children.Count : 0; } }
        /// <summary>
        /// Отслеживание изменение коллекции дочерних элементов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ChildrenCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ;
        }

        #region IProgress implementation

        /// <summary>
        /// Прогресс состояния группы
        /// </summary>
        [Magic]
        public virtual int Progress { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Magic]
        public virtual DataStatus Status { get; set; } = DataStatus.Wait;

        #endregion

        #region ITreeModel

        public virtual System.Collections.IEnumerable GetParentChildren(object parent)
        {
            if (parent == null)
                return Children;

            var element = parent as IHierarchicalEmcosPoint;
            if (element == null)
                return null;
            else
                return element.Children;
        }

        public virtual bool HasParentChildren(object parent)
        {
            var element = parent as IHierarchicalEmcosPoint;
            if (element == null)
                return false;
            else
            {
                if (element.Children == null)
                    return false;
                else
                    if (element.Children.Count == 0)
                    return false;
                else
                    return true;
            }
        }

        #endregion

        [XmlIgnore]
        [IgnoreDataMember]
        public object Tag { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]
        public object ToolTip
        {
            get
            {
                return string.Format("ИД точки в Emcos Corporate:{0},\nName:{1}, TypeCode:{2}", Id, Name, TypeCode);
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
    /// <summary>
    /// Группа элементов
    /// </summary>
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
    /// <summary>
    /// Элемент группы
    /// </summary>
    public class EmcosPointElement : IEmcosElement
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string EcpName { get; set; }
        public ElementTypes Type { get; set; }
    }
    /// <summary>
    /// Тип элемента
    /// </summary>
    public enum ElementTypes
    {
        GROUP, //0
        POINT, //1
        [Description("Подразделение")]
        DEPARTAMENT, //2
        [Description("Подстанция")]
        SUBSTATION, //3
        [Description("Ступень напряжения")]
        VOLTAGE, //4
        [Description("Секция шин")]
        SECTION, //5
        [Description("Трансформатор")]
        POWERTRANSFORMER, //6
        [Description("Трансформатор собственных нужд с секций")]
        UNITTRANSFORMER, //7
        [Description("Трансформатор собственных нужд с шин")]
        UNITTRANSFORMERBUS, //8
        [Description("Фидер")]
        FIDER, //9
        [Description("Трансформаторы")]
        POWERTRANSFORMERS, //10
        [Description("Собственные нужды")]
        AUXILIARY //11
    }
}
