namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum MeterEventType
    {
        None,
        Control,
        Change,
        Payment,
    }

    public class MeterEvent : IComparable, IComparable<MeterEvent>
    {
        public DateOnly Date { get; set; }

        public MeterEventType EventType { get; set; }

        public int Сonsumption { get; set; }

        public int LastElectricityReadings { get; set; }

        public MeterEvent(DateOnly date, MeterEventType meterEventType, int consumption, int lastElectricityReadings)
        {
            this.Date = date;
            this.EventType = meterEventType;
            this.Сonsumption = consumption;
            this.LastElectricityReadings = lastElectricityReadings;
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as MeterEvent);
        }

        public int CompareTo(MeterEvent other)
        {
            if (other == null)
            {
                return 1;
            }

            return this.Date.CompareTo(other.Date);
        }
    }
}
