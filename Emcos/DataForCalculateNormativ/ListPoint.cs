using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    [DataContract(Name = "EmcosPoint")]
    public class ListPoint
    {
        [DataMember]
        public decimal ParentId { get; set; }
        [DataMember]
        public string ParentTypeCode { get; set; }
        [DataMember]
        public string ParentTypeName { get; set; }
        [DataMember]
        public decimal Id { get; set; }
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
        public ObservableCollection<ListPoint> Items { get; set; }

        public ListPoint()
        {
            Items = new ObservableCollection<ListPoint>();
        }
    }
}