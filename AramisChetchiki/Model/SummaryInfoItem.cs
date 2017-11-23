using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace TMP.WORK.AramisChetchiki.Model
{
    [Serializable]
    [DataContract]
    public class SummaryInfoItem : IModel
    {
        [DataMember]
        public string FieldName { get; set; }
        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public string Info { get; set; }

        [DataMember]
        public ICollection<SummaryInfoChildItem> OnlyFirst10Items { get; set; }

        [DataMember]
        public ICollection<SummaryInfoChildItem> AllItems { get; set; }

        public SummaryInfoItem() { }
        public SummaryInfoItem(string header)
        {
            Header = header;
        }
        public SummaryInfoItem(string header, string info) : this(header)
        {
            Info = info;
        }
        [DataMember]
        public bool IsChecked { get; set; } = true;
    }

    [Serializable]
    [DataContract]
    public class SummaryInfoChildItem : IModel
    {
        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public uint Count { get; set; }
        [DataMember]
        public double Percent { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public bool IsEmpty { get; set; }
    }
}