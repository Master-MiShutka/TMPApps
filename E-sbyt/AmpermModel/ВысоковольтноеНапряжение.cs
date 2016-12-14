using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class ВысоковольтноеНапряжение
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public int Напряжение { get; set; }
        public bool ПометкаУдаления { get; set; }
    }
}
