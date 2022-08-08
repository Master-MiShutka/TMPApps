using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class РасчётнаяТочка
    {
        public object Владелец { get; set; }
        public string ЗнакПотерь { get; set; }
        public int ид { get; set; }
        public int Код { get; set; }
        public string Наименование { get; set; }
        public Напряжение Напряжение { get; set; }
        public Напряжение НапряжениеБП { get; set; }
        public int Номер { get; set; }
        public int номерТР { get; set; }
        public ОбъектУчёта ОбъектУчёта { get; set; }
        public bool ПометкаУдаления { get; set; }
        public string Примечание { get; set; }
        public РасчётнаяТочка Родитель { get; set; }
        public int узел { get; set; }
        public int уровень { get; set; }
    }
}
