using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace TMP.WORK.AramisChetchiki.Model
{
    [DataContract]
    [KnownType(typeof(Meter))]
    public class Data
    {
        [IgnoreDataMember]
        private DateTime _date;

        [DataMember]
        public Departament Departament { get; set; }

        [DataMember]
        public Version Version { get; set; }
        [DataMember]
        public DateTime Date
        {
            get { if (_date == default(DateTime)) return new DateTime(1900, 1, 1); else return _date; } set { _date = value; }
        }
        [DataMember]
        public ICollection<Meter> Meters { get; set; }
        [DataMember]
        public ICollection<ChangesOfMeters> ChangesOfMeters { get; set; }
        [DataMember]
        public ObservableCollection<SummaryInfoItem> Infos { get; set; }

        public Data()
        {
            Version = new Version(1, 0);
            Departament = new Departament();
        }
    }
}
