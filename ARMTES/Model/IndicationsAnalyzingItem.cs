using System;
using System.Collections.Generic;

namespace TMP.ARMTES.Model
{
    public class IndicationsAnalyzingItem
    {
        public int DeviceId { get; set; }
        public int ElementId { get; set; }
        public string MeterType { get; set; }
        public string SerialNumber { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Flat { get; set; }
        public string PersonalAccount { get; set; }
        public string SubscriberType { get; set; }

        public List<IndicationPointViewItem> IndicationPointViewItems { get; set; }

        public List<IndicationPointItem> IndicationPointItems { get; set; }

        public IndicationsAnalyzingItem()
        {
            IndicationPointViewItems = new List<IndicationPointViewItem>();
        }
    }

    public class IndicationPointViewItem
    {
        public string TimeStamp { get; set; }
        public float? Value { get; set; }
    }

    public class IndicationPointItem
    {
        public DateTime TimeStamp { get; set; }
        public float? Value { get; set; }
    }
}
