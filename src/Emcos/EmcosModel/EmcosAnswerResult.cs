using System;

namespace TMP.Work.Emcos.Model
{
    [Serializable]
    public class STPLDataResult
    {
        public int POINT_ID { get; set; }
        public string POINT_CODE { get; set; }
        public int ML_ID { get; set; }
        public DateTime PL_T { get; set; }
        public DateTime BT { get; set; }
        public DateTime ET { get; set; }
        public decimal PL_V { get; set; }
    }

    [Serializable]
    public class PointInfo
    {
        public int POINT_ID { get; set; }
        public string POINT_CODE { get; set; }
        public string POINT_NAME { get; set; }
    }
}
