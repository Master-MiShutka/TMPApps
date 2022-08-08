using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class Расчёт
    {
        public int годРП { get; set; }
        public int месяцРП { get; set; }
        public DateTime Дата1 { get; set; }
        public DateTime Дата2 { get; set; }
        public object Документ { get; set; }
        public int КР { get; set; }
        public int КТ { get; set; }
        public decimal Показание1 { get; set; }
        public decimal Показание2 { get; set; }
        public decimal ПоказаниеК { get; set; }
        public decimal ПоказаниеР { get; set; }
        public decimal ПотериИтого { get; set; }
        public decimal РасходИтого { get; set; }
        public decimal РасходПолный { get; set; }
        public decimal РасходСобственный { get; set; }
        public РасчётнаяТочка РасчётнаяТочка { get; set; }
        public РасчётныйПериод РП { get; set; }
        public bool состояние { get; set; }
        public СпособРасчёта СпособРасчёта { get; set; }
        public Счётчик Счётчик { get; set; }
    }
}
