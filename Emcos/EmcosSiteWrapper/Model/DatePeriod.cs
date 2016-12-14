using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace TMP.Work.Emcos.Model
{
    [DataContract(Namespace = "http://tmp.work.balans-substations.com")]
    public class DatePeriod
    {
        private IList<DateTime> _dates = null;
        [DataMember]
        [Newtonsoft.Json.JsonConverter(typeof(CustomJsonDateTimeFormat), "yyyy-MM-ddT00:00:00")]
        public DateTime StartDate { get; set; }
        [DataMember]
        [Newtonsoft.Json.JsonConverter(typeof(CustomJsonDateTimeFormat), "yyyy-MM-ddT00:00:00")]
        public DateTime EndDate { get; set; }
        public DatePeriod()
        {

        }
        public DatePeriod(DateTime? start, DateTime? end)
        {
            StartDate = start.HasValue == false ? DateTime.Today : start.Value;
            EndDate = end.HasValue == false ? DateTime.Today : end.Value;
        }
        public DatePeriod(DateTime start, DateTime end)
        {
            StartDate = start == null ? DateTime.Today : start;
            EndDate = end == null ? DateTime.Today : end;
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public IList<DateTime> Dates
        {
            get
            {
                if (StartDate == null || EndDate == null)
                    _dates = new List<DateTime>();
                if (_dates == null)
                {
                    _dates = new List<DateTime>();
                    for (DateTime day = StartDate; day <= EndDate; day = day.AddDays(1))
                        _dates.Add(day);
                }
                return _dates;
            }
        }
        public override string ToString()
        {
            return String.Format("{0:dd-MMMM-yyyy}-{1:dd-MMMM-yyyy}", StartDate, EndDate);
        }

        public override bool Equals(object obj)
        {
            var d2 = obj as DatePeriod;
            if (d2 == null)
                return false;
            return this.ToString().Equals(d2.ToString());
        }

        public string GetFriendlyDateRange()
        {
            var c = System.Globalization.CultureInfo.CurrentUICulture;

            if (StartDate == EndDate)
                return StartDate.ToString("dd MMMM, yyyy", c.DateTimeFormat);

            var lastDayOfMonth = DateTime.DaysInMonth(EndDate.Year, EndDate.Month);

            if (StartDate.Month == EndDate.Month && StartDate.Day == 1 && (EndDate.Day == DateTime.Today.Day || EndDate.Day == lastDayOfMonth))
                return StartDate.ToString("MMMM", c.DateTimeFormat) + " " + EndDate.ToString("yyyy", c.DateTimeFormat);

            if (StartDate.Year == EndDate.Year && StartDate.Day == 1 && (EndDate.Day == DateTime.Today.Day || EndDate.Day == lastDayOfMonth))
                return String.Format("{0:MMMM}–{1:MMMM} {0:yyyy}", StartDate, EndDate);

            if (StartDate.Day == 1 && EndDate.Day == DateTime.Today.Day)
                return StartDate.ToString("MMMM yyyy", c.DateTimeFormat) + "-" + EndDate.ToString("MMMM yyyy", c.DateTimeFormat);

            if (StartDate.Month == EndDate.Month && StartDate.Year == EndDate.Year)
                return StartDate.ToString("dd") + "–" + EndDate.ToString("dd MMMM, yyyy", c.DateTimeFormat);

            if (StartDate.Month == EndDate.Month)
                return String.Format("{0:dd}–{1:dd} {0:MMMM} {0:yyyy},{1:yyyy}", StartDate, EndDate);

            if (StartDate.Year == EndDate.Year)
                return String.Format("{0:dd MMMM}–{1:dd MMMM} {0:yyyy}", StartDate, EndDate);

            return String.Format("{0:dd MMMM yyyy}–{1:dd MMMM yyyy}", StartDate, EndDate);
        }

        public string GetFileNameForSaveSession()
        {
            return String.Format("{0:dd-MM-yyyy}_{1:dd-MM-yyyy}", StartDate, EndDate);
        }
    }
}
