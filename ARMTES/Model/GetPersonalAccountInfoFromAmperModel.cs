using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TMP.ARMTES.Model
{
    [Serializable]
    [DataContract]
    public class AmperGetPointInfo
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "found")]
        public string Found { get; set; }
        [DataMember(Name = "dogovor")]
        public ContractItem Dogovor { get; set; }
        [DataMember(Name = "object")]
        public ObjectItem Object { get; set; }
        [DataMember(Name = "point")]
        public PointItem Point { get; set; }
    }
    [DataContract]
    public class ContractItem
    {
        [DataMember(Name = "nomer")]
        public string ContractNumber { get; set; }
        [DataMember(Name = "RES")]
        public string Res { get; set; }
        [DataMember(Name = "abonent")]
        public SubscriberItem Subscriber { get; set; }
    }
    [DataContract]
    public class SubscriberItem
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "fullname")]
        public string FullName { get; set; }
        [DataMember(Name = "shortname")]
        public string ShortName { get; set; }
        [DataMember(Name = "address")]
        public string Adress { get; set; }
    }
    [DataContract]
    public class ObjectItem
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "level")]
        public string Level { get; set; }
        [DataMember(Name = "parent")]
        public string Parent { get; set; }
        [DataMember(Name = "address")]
        public string Adress { get; set; }
        [DataMember(Name = "RES")]
        public string Res { get; set; }
        [DataMember(Name = "town")]
        public string Town { get; set; }
    }
    [DataContract]
    public class PointItem
    {
        [DataMember(Name = "id")]
        public ulong Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "level")]
        public string Level { get; set; }
        [DataMember(Name = "parentid")]
        public string ParentId { get; set; }
        [DataMember(Name = "PS")]
        public string Substation { get; set; }
        [DataMember(Name = "fider")]
        public string Fider { get; set; }
        [DataMember(Name = "TP")]
        public string TransformerSubstation { get; set; }
        [DataMember(Name = "pribor")]
        public CounterItem Counter { get; set; }
    }
    [DataContract]
    public class CounterItem
    {
        [DataMember(Name = "tip")]
        public CounterItemType Type { get; set; }

        [DataMember(Name = "nomer")]
        public string SerialNumber { get; set; }
    }

    [DataContract]
    public class CounterItemType
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

    }
}
