using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.Model
{
    [Shared.Magic]
    public class BalanceSessionInfo : PropertyChangedBase
    {
        private DatePeriod _period = null;

        /// <summary>
        /// Версия данных
        /// </summary>
        [DataMember]
        public Version Version { get; set; } = new Version("1.1");

        /// <summary>
        /// Имя файла с сессией
        /// </summary>
        [DataMember]
        public string FileName { get; set; }
        /// <summary>
        /// Дата последнего изменения
        /// </summary>
        [DataMember]
        public DateTime LastModifiedDate { get; set; }
        /// <summary>
        /// Размер файла
        /// </summary>
        [IgnoreDataMember]
        public long FileSize { get; set; }
        /// <summary>
        /// Признак загруженной сеесии
        /// </summary>
        [IgnoreDataMember]
        public bool IsLoaded { get; set; }
        /// <summary>
        /// Временной период хранящихся данных
        /// </summary>
        [DataMember]
        [Shared.Magic]
        public DatePeriod Period
        {
            get
            {
                return _period;
            }
            set
            {
                if (_period != null)
                    if (_period.Equals(value)) return;
                _period = value;
                RaisePropertyChanged("Period");
                RaisePropertyChanged("Title");
            }
        }
        /// <summary>
        /// Заголовок
        /// </summary>
        [IgnoreDataMember]
        public string Title
        {
            get
            {
                return Period == null
                  ? Strings.DatePeriodNotDefined
                  : String.Format(Strings.FormatDataByPeriod, Period?.GetFriendlyDateRange());
            }
        }
    }
}
