using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    [Serializable]
    public class EmcosReportType
    {
        public int RP_TYPE_ID { get; set; }
        public string RP_TYPE_NAME { get; set; }
        public static string TYPE { get; } = "RP_TYPE";
        public bool RP_PUBLIC { get; set; }
    }
    [Serializable]
    public class EmcosReport
    {
        public int RP_ID { get; set; }
        public int RP_TYPE_ID { get; set; }
        public string RP_NAME { get; set; }
        public bool RP_PUBLIC { get; set; }
        public string RP_DESCRIPTION { get; set; }
        public int RPF_ID { get; set; }
        public bool RP_LOG_ENABLED { get; set; }
        public string USER_NAME { get; set; }
        public static string TYPE { get; } = "RP";
    }
}
