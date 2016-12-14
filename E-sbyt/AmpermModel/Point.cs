using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Point
    {
        public string id { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public string parent { get; set; }
        public string parentid { get; set; }
        public bool turnon { get; set; }
        public string U { get; set; }
        public string UBP { get; set; }
        public string power { get; set; }
        public string PS { get; set; }
        public string fider { get; set; }
        public string TP { get; set; }
        public Pribor pribor { get; set; }
        public string poteri { get; set; }
        public object rezim { get; set; }
        public string targroup { get; set; }
        public bool vnpotr { get; set; }
        public bool sobstvnuzd { get; set; }
        public bool osnproizv { get; set; }
        public string otddogovor { get; set; }
        public string sposobrasceta { get; set; }
        public int kdoli { get; set; }
        public int fixrashod { get; set; }
        public string remark { get; set; }
    }
}
