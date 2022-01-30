namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Runtime.InteropServices;

    public enum MeterEventType : sbyte
    {
        None = 0,
        Control = 1,
        Change = 2,
        Payment = 3,
    }

    [MessagePack.MessagePackObject]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct MeterEvent : IComparable, IComparable<MeterEvent>
    {
        [MessagePack.Key(0)]
        public DateOnly Date { get; set; }

        [MessagePack.Key(1)]
        public MeterEventType EventType { get; set; }

        [MessagePack.Key(2)]
        public uint Сonsumption { get; set; }

        [MessagePack.Key(3)]
        public uint LastElectricityReadings { get; set; }

        public MeterEvent(DateOnly date, MeterEventType meterEventType, uint consumption, uint lastElectricityReadings)
        {
            this.Date = date;
            this.EventType = meterEventType;
            this.Сonsumption = consumption;
            this.LastElectricityReadings = lastElectricityReadings;
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo((MeterEvent)obj);
        }

        public int CompareTo(MeterEvent other)
        {
            return this.Date.CompareTo(other.Date) ^ this.EventType.CompareTo(other.EventType);
        }
    }
}
