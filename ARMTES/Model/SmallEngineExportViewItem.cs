using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TMP.ARMTES.Model
{
    [DataContract]
    public class SmallEngineExportViewItem
    {
        [DataMember]
        public int OrderNumber { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DialNumber { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember(Name = "LastSession")]
        private string LastSessionDateAsString { get; set; }
        [IgnoreDataMember]
        public DateTime? LastSession
        {
            get
            {
                if (String.IsNullOrWhiteSpace(LastSessionDateAsString))
                    return null;
                else
                    return DateTime.ParseExact(LastSessionDateAsString, "yyyy'-'MM'-'dd'T'HH':'mm':'ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            set
            {
                LastSessionDateAsString = value?.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff");
            }
        }
        [DataMember]
        public List<SmallEngineCounterViewItem> Counters { get; set; }

        public SmallEngineExportViewItem()
        {
            Counters = new List<SmallEngineCounterViewItem>();
        }
    }
    [DataContract]
    public class SmallEngineCounterViewItem
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string SerialNumber { get; set; }
        [DataMember]
        public string NetworkAdress { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public float? PreviousIndications { get; set; }
        [DataMember]
        public float? NextIndications { get; set; }
        [DataMember]
        public float? IndicationsDifference { get; set; }
        [DataMember]
        public float? PreviousIndicationsDifference { get; set; }
    }


    public class SmallEngineExportViewItemWithCounter
    {
        public string Status { get; set; }
        public string Name { get; set; }
        public string DialNumber { get; set; }
        public DateTime? LastSession { get; set; }
        public string CounterName { get; set; }
        public string SerialNumber { get; set; }
        public string NetworkAdress { get; set; }
        public string CounterStatus { get; set; }
        public float? PreviousIndications { get; set; }
        public float? NextIndications { get; set; }
        public float? IndicationsDifference { get; set; }
        public float? PreviousIndicationsDifference { get; set; }
    }
}
