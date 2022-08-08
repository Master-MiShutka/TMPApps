using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class ДолжностноеЛицо
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public bool ПометкаУдаления { get; set; }
        public string Email { get; set; }
        public Адрес Адрес { get; set; }
        public Должность Должность { get; set; }
        public string Подразделение { get; set; }
        public string Телефон { get; set; }
        public string Факс { get; set; }
        public string СтрокаДоверенность { get; set; }
        public string Примечание { get; set; }
    }
}
