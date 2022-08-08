using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class РасчётныйПериод
    {
        public DateTime ДатаРасчёта { get; set; }
        public Договор Договор { get; set; }
        public int Код { get; set; }
        public string Наименование { get; set; }
        public ОтчётныйПериод ОтчётныйПериод { get; set; }
        public Decimal РасходПолный { get; set; }
        public Decimal РасходФакт { get; set; }
        public DateTime РПДатаНач { get; set; }
        public DateTime РПДатаКон { get; set; }
    }
}
