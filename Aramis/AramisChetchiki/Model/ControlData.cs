namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [MessagePack.MessagePackObject]
    [System.Diagnostics.DebuggerDisplay("ID={Лицевой}")]
    public class ControlData : IModelWithPersonalId
    {
        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormatAttribute]
        [MessagePack.Key(0)]
        public ulong Лицевой
        {
            get;
            set;
        }

        [DisplayName("Данные")]
        [MessagePack.Key(1)]
        public IEnumerable<MeterControlData> Data
        {
            get;
            set;
        }

        [DisplayName("ФИО оператора")]
        [MessagePack.Key(2)]
        public string Оператор
        {
            get;
            set;
        }
    }

    [MessagePack.MessagePackObject]
    [System.Diagnostics.DebuggerDisplay("Date={Date}, Value={Value}, Operator={Operator}")]
    public class MeterControlData : IModelWithMeterLastReading
    {
        public MeterControlData(DateOnly date, uint value, string @operator)
        {
            this.Date = date;
            this.Value = value;
            this.Operator = @operator;
        }

        [MessagePack.Key(0)]
        public DateOnly Date
        {
            get;
            set;
        }

        [MessagePack.Key(1)]
        public uint Value
        {
            get;
            set;
        }

        [MessagePack.Key(2)]
        public string Operator
        {
            get;
            set;
        }

        [MessagePack.IgnoreMember]
        public uint LastReading => this.Value;
    }
}
