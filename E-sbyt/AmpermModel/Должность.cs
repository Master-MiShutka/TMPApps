using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Должность
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public string НаименованиеДательный { get; set; }
        public string НаименованиеРодительный { get; set; }
        public bool ПометкаУдаления { get; set; }
    }
}
