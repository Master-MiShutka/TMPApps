using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.XmlConfiguration;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    [Serializable]
    public class ListPoint
    {
        [DataMember]
        public int ParentId { get; set; }
        [DataMember]
        public string ParentTypeCode { get; set; }
        [DataMember]
        public string ParentName { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsGroup { get; set; }
        [DataMember]
        public string TypeCode { get; set; }
        [DataMember]
        public string EсpName { get; set; }
        [DataMember]
        public Model.ElementTypes Type { get; set; }
        [DataMember]
        public bool Checked { get; set; }
        [DataMember]
        public IList<ListPoint> Items { get; set; }

        public ListPoint()
        {
            Items = new List<ListPoint>();
        }
        public override string ToString()
        {
            return string.Format("Id:{0}, Name:{1}, TypeCode:{2}",
                Id,
                Name,
                TypeCode);
        }
    }
}