using System.Collections.Generic;

namespace TMP.ARMTES.Model
{
    public class ChartStatisticsViewModel
    {
        public string MinDate { get; set; }
        public string MaxDate { get; set; }

        public string ChartTitle { get; set; }

        public string ParameterTitle { get; set; }

        public List<FlatViewElement> Flats { get; set; }
        public List<ChartStatisticsViewPoint> ChartStatisticsViewPoints { get; set; }
        public ChartStatisticsViewModel()
        {
            ChartStatisticsViewPoints = new List<ChartStatisticsViewPoint>();

            Flats = new List<FlatViewElement>();
        }
    }
    public class ChartStatisticsViewPoint
    {
        public string TimeStamp { get; set; }
        public double BalanceValue { get; set; }
    }
    public class FlatViewElement
    {
        public int FlatId { get; set; }
        public string FlatName { get; set; }
    }
}
