namespace TMP.UI.WPF.Controls.TableView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
            this.MinOfValues = min;
            this.MaxOfValues = max;
            this.SumOfValues = sum;
            this.AverageOfValues = ave;

            if (this.MinOfValues.HasValue == false)
            {
                this.MinOfValues = 0.0;
            }

            if (this.MaxOfValues.HasValue == false)
            {
                this.MaxOfValues = 0.0;
            }

            if (this.MinOfValues.Value < 0)
            {
                this.AbsMinPlusMax = Math.Abs(this.MinOfValues.Value) + (this.MaxOfValues.Value < 0.0 ? 0.0 : this.MaxOfValues.Value);
                this.NullRealative = Math.Abs(this.MinOfValues.Value) / this.AbsMinPlusMax;
            }
            else
            {
                this.AbsMinPlusMax = this.MaxOfValues.Value;
                this.NullRealative = 0.0;
            }

            System.Diagnostics.Debug.Assert(this.NullRealative <= 1.0);
        }
    }
}
