using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Dogovor
    {
        public string nomer { get; set;}
        public string tip { get; set; }
        public string status { get; set; }
        public DateTime data { get; set; }
        public byte srok { get; set; }
        public Osnovanie osnovanie { get; set; }
        public string direktor { get; set; }
        public string energetik { get; set; }
        public string buhgalter { get; set; }
        public string RES { get; set; }
        public string gruppapotr { get; set; }
        public string gruppaministerstv { get; set; }
        public string sposobopl { get; set; }
        public string rscet { get; set; }
        public string bank { get; set; }
        public string CBU { get; set; }
        public Abonent abonent { get; set; }
    }
}
