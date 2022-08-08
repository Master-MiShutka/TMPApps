using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class ИзмерительныйТрансформатор
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public bool ПометкаУдаления { get; set; }
        public DateTime ДатаПоверки{ get; set; }
        public string ЗаводскойНомер { get; set; }

    }
}
