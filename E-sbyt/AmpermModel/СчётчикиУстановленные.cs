using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class СчётчикиУстановленные
    {
        public DateTime Дата { get; set; }
        public ДокументыТипы Документ { get; set; }
        public int КР { get; set; }
        public int КТ { get; set; }
        public DateTime Период { get; set; }
        public Decimal ПоказаниеПрибора { get; set; }
        public Decimal ПоказаниеРасчётное { get; set; }
        public РасчётнаяТочка РасчётнаяТочка { get; set; }
        public Счётчик Счётчик { get; set; }
        public ИзмерительныйТрансформатор ТТ1 { get; set; }
        public ИзмерительныйТрансформатор ТТ2 { get; set; }
        public ИзмерительныйТрансформатор ТТ3 { get; set; }
        public ИзмерительныйТрансформатор ТН { get; set; }
    }
}
