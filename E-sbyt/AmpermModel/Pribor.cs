using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Pribor
    {
        public string nomer { get; set; }
        public DateTime vipusk { get; set; }
        public DateTime poverka { get; set; }
        public string balans { get; set; }
        public DateTime datapok { get; set; }
        public Decimal pokazanie { get; set; }
        public PriborTip tip { get; set; }
    }
}
