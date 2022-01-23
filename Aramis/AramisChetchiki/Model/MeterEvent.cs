﻿namespace TMP.WORK.AramisChetchiki.Model
{
    using System;

    public enum MeterEventType
    {
        None,
        Control,
        Change,
        Payment,
    }

    [MessagePack.MessagePackObject]
    public class MeterEvent : IComparable, IComparable<MeterEvent>
    {
        [MessagePack.Key(0)]
        public DateOnly Date { get; set; }

        [MessagePack.Key(1)]
        public MeterEventType EventType { get; set; }

        [MessagePack.Key(2)]
        public int Сonsumption { get; set; }

        [MessagePack.Key(3)]
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
