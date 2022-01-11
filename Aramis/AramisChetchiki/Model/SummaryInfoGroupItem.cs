namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;

    public class SummaryInfoGroupItem
    {
        public object Key { get; set; }

        public bool HasEmptyValue { get; set; }

        public IList<Meter> Value { get; set; }

        public int Count { get; set; }

        public double Percent { get; set; }
    }
}
