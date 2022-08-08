using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class ИзмерительныйТрансформаторМарка
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public bool ПометкаУдаления { get; set; }
        public int I1 { get; set; }
        public int I2 { get; set; }
        public int U1 { get; set; }
        public int U2 { get; set; }
        public ИзмерительныйТрансформаторТип ИзмерительныеТрансформаторыТип { get; set; }
        public КлассТочности КлассТочности { get; set; }
        public int КоэффициентТрансформации { get; set; }
        public НоминальноеНапряжение Напряжение { get; set; }
        public Фазность Фазность { get; set; }
    }
}
