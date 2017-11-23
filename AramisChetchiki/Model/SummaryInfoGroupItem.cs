using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.WORK.AramisChetchiki.Model
{
    public class SummaryInfoGroupItem
    {
        public object Key { get; set; }
        public bool HasEmptyValue { get; set; }
        public IList<Meter> Value { get; set; }
        public int Count { get; set; }
    }
}
