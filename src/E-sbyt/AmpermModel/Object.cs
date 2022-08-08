using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Object
    {
        public string name { get; set; }
        public int level { get; set; }
        public string parent { get; set; }
        public string address { get; set; }
        public string town { get; set; }
        public string gruppa { get; set; }
        public string ministry { get; set; }
        public string RES { get; set; }
        public string kategory { get; set; }
        public bool zones { get; set; }
        public string sposobrasceta { get; set; }
        public bool ASKUE { get; set; }
        public bool RAS { get; set; }
        public string otddogovor { get; set; }
        public object dogovorporucitelstva { get; set; }
        public string subabonent { get; set; }
        public string arendator { get; set; }
    }
}
