using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Договор
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public int Номер { get; set; }
        public string НомерПравыйКрай { get; set; }
        public bool ПометкаУдаления { get; set; }
        public int ПризнакПереоформления { get; set; }
        public bool ПН { get; set; }
        public object Абонент { get; set; }
        public bool Арендатор { get; set; }
        public ГруппаМинистерств ГруппаМинистерств { get; set; }
        public ГруппаПотребления ГруппаПотребления { get; set; }
        public DateTime ДатаЗаключения { get; set; }
        public DateTime ДатаНачалаДействия { get; set; }
        public DateTime ДатаПодписания { get; set; }
        public DateTime ДатаПродления { get; set; }
        public DateTime ДатаРасторжения { get; set; }
        public ДолжностноеЛицо Директор { get; set; }
        public ДолжностноеЛицо Энергетик { get; set; }
        public ДолжностноеЛицо Бухгалтер { get; set; }
        public object Договорник { get; set; }
        public string Примечание { get; set; }
        public int ПрисоединённаяМощность { get; set; }
        public object РЭС { get; set; }
        public int СрокДействия { get; set; }
        public object СтатусДоговора { get; set; }
        public bool Субабонент { get; set; }
        public ДоговорТип ТипДоговора { get; set; }
        public string Филиал { get; set; }
    }
}
