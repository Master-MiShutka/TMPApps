using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Подстанция
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public ВысоковольтноеНапряжение Напряжение { get; set; }
        public string Номер { get; set; }
        public int номерпп { get; set; }
        public bool ПометкаУдаления { get; set; }
        public Подстанция Родитель { get; set; }


    }
}
