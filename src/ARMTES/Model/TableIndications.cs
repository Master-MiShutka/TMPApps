using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.ARMTES.Model
{
    public class TableIndications
    {
        public int ParameterId { get; set; }
        public string TableTitle { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<string> AllDatesRange { get; set; }
        public List<TableIndicationItem> Items { get; set; }
        public string TableParameterTitle { get; set; }

        public TableIndications()
        {
            AllDatesRange = new List<string>();

            Items = new List<TableIndicationItem>();
        }
    }

    public class TableIndicationItem
    {
        public List<TableIndicationRowItem> Rows { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string StartDateString { get; set; }
        public string EndDateString { get; set; }
        public string TableDateTitle { get; set; }


        public TableIndicationItem()
        {
            Rows = new List<TableIndicationRowItem>();
        }
    }

    public class TableIndicationRowItem
    {
        public string ParameterName { get; set; }
        public List<TableIndicationCellItem> Cells { get; set; }
    }

    public class TableIndicationCellItem
    {
        public string Value { get; set; }
        public ValueStatus ValueStatus { get; set; }
        public DateTime TimeStamp { get; set; }
        public string TimeStampString { get; set; }
        public int OutOfMonthPosition { get; set; }
        public bool IsWeekendDay { get; set; }
    }

    public enum ValueStatus
    {
        NotInRange,
        NotInDateRange,
        EmptyValue,
        InvalidValue,
        ValidValue
    }
}
