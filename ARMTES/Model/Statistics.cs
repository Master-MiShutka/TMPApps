using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.ARMTES.Model
{
    public class Statistics
    {
        public int ModemsCount { get; set; }

        public int AnsweredModemsCount { get; set; }

        public int NotAnsweredModemsCount { get; set; }

        public int UspdCount { get; set; }

        public int AnsweredUspdCount { get; set; }

        public int NotAnsweredUspdCount { get; set; }

        public int CountersCount { get; set; }

        public int AnsweredCountersCount { get; set; }

        public int NotAnsweredCountersCount { get; set; }
        /// <summary>
        /// Количество недостающих данных
        /// </summary>
        public int MissingDataCount { get; set; }
        /// <summary>
        /// Процент опроса
        /// </summary>
        public int PercentageOfTheSurvey { get { return CountersCount == 0 ? 0 : 100 * AnsweredCountersCount / CountersCount; } }
    }
}
