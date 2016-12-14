using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.Accounting.Model
{
    using TMP.Work.ESbyt.AmpermModel;
    public class PointInfo
    {
        public string status { get; set; }
        public int found { get; set; }
        public Dogovor dogovor { get; set; }
        public Object Object {get; set;}
        public Point point { get; set; }
    }
}
