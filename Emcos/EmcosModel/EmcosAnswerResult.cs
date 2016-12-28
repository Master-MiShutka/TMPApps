using System;

namespace TMP.Work.Emcos.Model
{
    [Serializable]
    public class STPLDataResult
    {
        public decimal POINT_ID { get; set; }
        public string POINT_CODE { get; set; }
        public decimal ML_ID { get; set; }
        public DateTime PL_T { get; set; }
        public DateTime BT { get; set; }
        public DateTime ET { get; set; }
        public decimal PL_V { get; set; }
    }

    [Serializable]
    public class PointInfo
    {
        public decimal POINT_ID { get; set; }
        public string POINT_CODE { get; set; }
        public string POINT_NAME { get; set; }
    }
}
