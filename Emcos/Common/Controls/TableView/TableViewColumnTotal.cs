using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Wpf.Common.Controls.TableView
{
    public class TableViewColumnTotal
    {
        public double? MinOfValues { get; set; }
        public double? MaxOfValues { get; set; }
        public double? SumOfValues { get; set; }
        public double? AverageOfValues { get; set; }

        public double NullRealative { get; private set; }
        public double AbsMinPlusMax { get; private set; }

        public TableViewColumnTotal(double? min, double? max, double? sum, double? ave)
        {
            MinOfValues = min;
            MaxOfValues = max;
            SumOfValues = sum;
            AverageOfValues = ave;

            if (MinOfValues.HasValue == false) MinOfValues = 0.0;
            if (MaxOfValues.HasValue == false) MaxOfValues = 0.0;            

            if (MinOfValues.Value < 0)
            {
                AbsMinPlusMax = Math.Abs(MinOfValues.Value) + (MaxOfValues.Value < 0.0 ? 0.0 : MaxOfValues.Value);
                NullRealative = Math.Abs(MinOfValues.Value) / AbsMinPlusMax;
            }
            else
            {
                AbsMinPlusMax = MaxOfValues.Value;
                NullRealative = 0.0;
            }
            System.Diagnostics.Debug.Assert(NullRealative <= 1.0);
        }
    }
}
