using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using TMP.ARMTES.Model;

namespace TMP.ARMTES
{
    [DataContract]
    [KnownType(typeof(SmallEngineDataModel))]

    class Data
    {
        [IgnoreDataMember]
        private DateTime _fromDate;
        private DateTime _toDate;

        [DataMember]
        public DateTime FromDate
        {
            get { if (_fromDate == default(DateTime)) return new DateTime(1900, 1, 1); else return _fromDate; }
            set { _fromDate = value; }
        }
        [DataMember]
        public DateTime ToDate
        {
            get { if (_toDate == default(DateTime)) return new DateTime(1900, 1, 1); else return _toDate; }
            set { _toDate = value; }
        }

        [DataMember]
        public IList<SmallEngineDataModel> Items { get; set; }

        [DataMember]
        public Profile Profile { get; set; }

        [DataMember]
        public IList<EnterpriseViewItem> Enterprises { get; set; }

        [DataMember]
        public IList<SmallEngineExportViewItem> SEO { get; set; }

        [DataMember]
        public List<ConfigurationContainer> Configs { get; set; }

    }
}
