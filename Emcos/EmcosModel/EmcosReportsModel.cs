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

        public EmcosReportType() { }
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

        public EmcosReport() { }

        public override string ToString()
        {
            return string.Format("Id:{0}, Name:{1}, TypeId:{2}",
                RP_ID,
                RP_NAME,
                RP_TYPE_ID);
        }
        public override bool Equals(object obj)
        {
            EmcosReport o = obj as EmcosReport;
            if (o == null) return false;

            return this.RP_ID == o.RP_ID && this.RP_TYPE_ID == o.RP_TYPE_ID && this.RP_NAME == o.RP_NAME; 
        }
        public override int GetHashCode()
        {
            return RP_ID;
        }
    }
}
