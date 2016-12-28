using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    public interface IBalansItem : IEmcosPoint, System.ComponentModel.INotifyPropertyChanged, IDisposable
    {
        string Correction { get; }
        [DataMember()]
        double? Eplus { get; set; }
        double? EnergyIn { get; }
        [DataMember()]
        double? AddToEplus { get; set; }
        [DataMember()]
        double? MonthEplus { get; set; }
        [DataMember()]
        IList<double?> DailyEplus { get; set; }
        IList<Value> DailyEplusValues { get; }

        double? DailyEplusValuesSum { get; }
        double? DailyEplusValuesAverage { get; }
        double? DailyEplusValuesMin { get; }
        double? DailyEplusValuesMax { get; }

        [DataMember()]
        double? MonthEminus { get; set; }
        [DataMember()]
        double? Eminus { get; set; }
        double? EnergyOut { get; }
        [DataMember()]
        double? AddToEminus { get; set; }
        [DataMember()]
        IList<double?> DailyEminus { get; set; }
        IList<Value> DailyEminusValues { get; }

        double? DailyEminusValuesSum { get; }
        double? DailyEminusValuesAverage { get; }
        double? DailyEminusValuesMin { get; }
        double? DailyEminusValuesMax { get; }


        [DataMember()]
        ElementTypes Type { get; }

        DataStatus Status { get; set; }

        bool NotFullData { get; }
        bool NotFullDataPlus { get; }
        bool NotFullDataMinus { get; }
        string DataPlusStatus { get; }
        string DataMinusStatus { get; }
        [DataMember()]
        string Description { get; set; }
        Substation Substation { get; }
        void SetSubstation(Substation substation);

        bool UseMonthValue { get; set; }
        double? DayEminusValue { get; set; }
        double? DayEplusValue { get; set; }

        void Clear();

        IBalansItem Copy();
    }
}
