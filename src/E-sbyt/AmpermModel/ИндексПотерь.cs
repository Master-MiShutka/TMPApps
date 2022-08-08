using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.ESbyt.AmpermModel
{
    public class ИндексПотерь
    {
        public int Код { get; set; }
        public string Наименование { get; set; }
        public bool ПометкаУдаления { get; set; }
        public bool Высокая { get; set; }
        public bool Низкая { get; set; }
        public string Примечания { get; set; }
        public string Сокращение { get; set; }
        public bool Трансформатор { get; set; }
    }
}
