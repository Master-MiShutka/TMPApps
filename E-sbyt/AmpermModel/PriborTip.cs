using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class PriborTip
    {
        public string name { get; set; }
        public string princip { get; set; }
        public string tarif { get; set; }
        public string Inom { get; set; }
        public string Unom { get; set; }
        public int faz { get; set; }
        public int digits { get; set; }
        public int decimals { get; set; }
        public string klass { get; set; }
        public int periodpoverki { get; set; }
    }
}
