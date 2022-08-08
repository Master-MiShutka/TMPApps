using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Адрес
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public bool ПометкаУдаления { get; set; }
        public string Примечание { get; set; }
        public int ПочтовыйИндекс { get; set; }
    }
}
