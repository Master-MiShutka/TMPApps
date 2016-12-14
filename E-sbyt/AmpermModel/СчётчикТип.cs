using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class СчётчикТип
    {
        public ДиапазонПоверки ДиапазонПоверки { get; set; }
        public КлассТочности КлассТочности { get; set; }
        public int Код { get; set; }
        public int КоличествоДесятичныхЗнаков { get; set; }
        public string Наименование { get; set; }
        public НоминальноеНапряжение НоминальноеНапряжение { get; set; }
        public bool ПометкаУдаления { get; set; }
        public int Разрядность { get; set; }
        public СчётчикТип Родитель { get; set; }
        public СчётчикПринципДействия СчётчикиПринципыДействия { get; set; }
        public Фазность Фазность { get; set; }
    }
}
