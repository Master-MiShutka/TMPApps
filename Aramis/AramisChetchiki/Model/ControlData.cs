namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [MessagePack.MessagePackObject]
    public class ControlData : IModelWithPersonalId
    {
        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormat]
        [MessagePack.Key(0)]
        public ulong Лицевой
        {
            get;
            set;
        }

        [DisplayName("Данные")]
        [MessagePack.Key(1)]
        public IEnumerable<KeyValuePair<DateOnly, int>> Data
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
}
