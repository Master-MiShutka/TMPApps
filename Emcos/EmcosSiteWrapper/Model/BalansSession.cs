using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace TMP.Work.Emcos.Model
{
    using Balans;
    [DataContract(Namespace = "http://tmp.work.balans-substations.com")]
    public class BalansSession : IBalansSession, IDisposable, INotifyPropertyChanged
    {
        private DatePeriod _period = null;
        [DataMember]
        public const string Version = "1.0";

        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public DateTime LastModifiedDate { get; set; }
        [IgnoreDataMember]
        public long FileSize { get; set; }
        [IgnoreDataMember]
        public bool IsLoaded { get; set; }
        [IgnoreDataMember]
        public string Title
        {
            get { return Period == null ? string.Empty : String.Format("Данные за период: {0}", Period.GetFriendlyDateRange()); }
        }
        [DataMember]
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

        [DataMember]
        public IList<Substation> Substations { get; set; }

        ~BalansSession()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (Substations != null)
                foreach (Substation s in Substations)
                    s.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #region INotifyPropertyChanged Members
        /// <summary>
        /// Raises the PropertyChange event for the property specified
        /// </summary>
        /// <param name="propertyName">Property name to update. Is case-sensitive.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
