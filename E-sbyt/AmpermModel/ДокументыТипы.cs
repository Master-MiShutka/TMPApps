using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class ДокументыТипы
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public bool ПометкаУдаления { get; set; }
        public object Родитель { get; set; }
    }
}
