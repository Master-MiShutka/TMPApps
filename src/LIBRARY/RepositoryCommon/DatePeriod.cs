namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    [MessagePack.MessagePackObject]
    public class DatePeriod : INotifyPropertyChanged
    {
        [IgnoreDataMember]
        private DateOnly startDate;
        [IgnoreDataMember]
        private DateOnly endDate;
        [IgnoreDataMember]
        private bool notDefined = true;
        [IgnoreDataMember]
        private IList<DateOnly> dates = null;

        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        [MessagePack.Key(0)]
        public DateOnly StartDate
        {
            get => this.startDate;
            set
            {
                if (value == this.endDate)
                {
                    return;
                }

                this.startDate = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Dates));
                this.NotDefined = false;
            }
        }

        [DataMember]
        [MessagePack.Key(1)]
        public DateOnly EndDate
        {
            get => this.endDate;
            set
            {
                if (value == this.endDate)
                {
                    return;
                }

                this.endDate = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Dates));
                this.NotDefined = false;
            }
        }

        [MessagePack.Key(2)]
        public bool NotDefined
        {
            get => this.notDefined;
            private set
            {
                if (value == this.notDefined)
                {
                    return;
                }

                this.notDefined = value;
                this.OnPropertyChanged();
            }
        }

        public DatePeriod()
        {
            this.StartDate = this.Today;
            this.EndDate = this.Today;
        }

        public DatePeriod(DateOnly start, DateOnly end)
        {
            this.StartDate = start == DateOnly.MinValue ? this.Today : start;
            this.EndDate = end == DateOnly.MinValue ? this.Today : end;
        }

        [IgnoreDataMember]
        public IList<DateOnly> Dates
        {
            get
            {
                if (this.StartDate == DateOnly.MinValue || this.EndDate == DateOnly.MinValue)
                {
                    this.dates = new List<DateOnly>();
                }

                if (this.dates == null)
                {
                    this.dates = new List<DateOnly>();
                    for (DateOnly day = this.StartDate; day <= this.EndDate; day = day.AddDays(1))
                    {
                        this.dates.Add(day);
                    }
                }

                return this.dates;
            }
        }

        public override string ToString()
        {
            return string.Format("{0:dd-MMMM-yyyy}-{1:dd-MMMM-yyyy}", this.StartDate, this.EndDate);
        }

        public override bool Equals(object obj)
        {
            return obj is DatePeriod d2 && this.ToString().Equals(d2.ToString());
        }

        public override int GetHashCode()
        {
            return this.StartDate.GetHashCode() | this.StartDate.GetHashCode();
        }

        public string GetFriendlyDateRange()
        {
            var c = System.Globalization.CultureInfo.CurrentUICulture;

            if (this.StartDate == this.EndDate)
            {
                return this.StartDate.ToString("dd MMMM, yyyy", c.DateTimeFormat);
            }

            var lastDayOfMonth = DateTime.DaysInMonth(this.EndDate.Year, this.EndDate.Month);

            if (this.StartDate.Month == this.EndDate.Month && this.StartDate.Day == 1 && (this.EndDate.Day == DateTime.Today.Day || this.EndDate.Day == lastDayOfMonth))
            {
                return this.StartDate.ToString("MMMM", c.DateTimeFormat) + " " + this.EndDate.ToString("yyyy", c.DateTimeFormat);
            }

            if (this.StartDate.Year == this.EndDate.Year && this.StartDate.Day == 1 && (this.EndDate.Day == DateTime.Today.Day || this.EndDate.Day == lastDayOfMonth))
            {
                return string.Format("{0:MMMM}–{1:MMMM} {0:yyyy}", this.StartDate, this.EndDate);
            }

            if (this.StartDate.Day == 1 && this.EndDate.Day == DateTime.Today.Day)
            {
                return this.StartDate.ToString("MMMM yyyy", c.DateTimeFormat) + "-" + this.EndDate.ToString("MMMM yyyy", c.DateTimeFormat);
            }

            if (this.StartDate.Month == this.EndDate.Month && this.StartDate.Year == this.EndDate.Year)
            {
                return this.StartDate.ToString("dd") + "–" + this.EndDate.ToString("dd MMMM, yyyy", c.DateTimeFormat);
            }

            if (this.StartDate.Month == this.EndDate.Month)
            {
                return string.Format("{0:dd}–{1:dd} {0:MMMM} {0:yyyy},{1:yyyy}", this.StartDate, this.EndDate);
            }

            if (this.StartDate.Year == this.EndDate.Year)
            {
                return string.Format("{0:dd MMMM}–{1:dd MMMM} {0:yyyy}", this.StartDate, this.EndDate);
            }

            return string.Format("{0:dd MMMM yyyy}–{1:dd MMMM yyyy}", this.StartDate, this.EndDate);
        }

        public string GetFileName()
        {
            return this.GetFriendlyDateRange();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static DatePeriod InitFromString(string datePeriodAsString)
        {
            if (string.IsNullOrWhiteSpace(datePeriodAsString))
            {
                return null;
            }

            string[] parts = datePeriodAsString.Split(new char[] { '-' });

            DateOnly start, end;

            CultureInfo culture = CultureInfo.InvariantCulture;

            DateOnly.TryParseExact(parts[0], "dd-MMMM-yyyy", culture, DateTimeStyles.None, out start);
            DateOnly.TryParseExact(parts[1], "dd-MMMM-yyyy", culture, DateTimeStyles.None, out end);

            return new DatePeriod(start, end);
        }

        private DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
    }
}
