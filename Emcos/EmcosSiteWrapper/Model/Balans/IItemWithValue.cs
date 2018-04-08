using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.Model.Balans
{
    interface IItemWithValue
    {
        string DifferenceBetweenDailySumAndMonthPlusToolTip { get; }
        string DifferenceBetweenDailySumAndMonthMinusToolTip { get; }

        IList<DataValue> DailyEplusValues { get; }
        IList<DataValue> DailyEminusValues { get; }

        double? DailyEplusValuesSum { get; }
        double? DailyEplusValuesAverage { get; }
        double? DailyEplusValuesMin { get; }
        double? DailyEplusValuesMax { get; }

        double? DailyEminusValuesSum { get; }
        double? DailyEminusValuesAverage { get; }
        double? DailyEminusValuesMin { get; }
        double? DailyEminusValuesMax { get; }
    }
}
