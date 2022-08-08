namespace TMP.WORK.AramisChetchiki
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class MeterEventsCollection : Collection<MeterEvent>
    {
        public MeterEventsCollection() { }

        public MeterEventsCollection(IEnumerable<MeterEvent> enumerable)
            : base(enumerable?.ToList())
        {
        }
    }
}
