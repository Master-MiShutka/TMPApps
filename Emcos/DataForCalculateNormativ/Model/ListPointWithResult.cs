using System;
using System.Collections.Generic;
using System.Linq;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public class ListPointWithResult : ListPoint
    {
        private string _resultName;
        private string _resultType;
        private object _resultValue;
        private ListPointStatus _status;

        public new IList<ListPointWithResult> Items { get; set; }

        public ListPointWithResult() { }
        public ListPointWithResult(ListPoint source)
        {
            this.ParentId = source.ParentId;
            this.ParentTypeCode = source.ParentTypeCode;
            this.ParentName = source.ParentName;
            this.Id = source.Id;
            this.Name = source.Name;
            this.IsGroup = source.IsGroup;
            this.TypeCode = source.TypeCode;
            this.EсpName = source.EсpName;
            this.Type = source.Type;
            this.Checked = source.Checked;
            if (source.Items != null)
                this.Items = source.Items.Select(i => new ListPointWithResult(i)).ToList<ListPointWithResult>();

            string name = source.Name;
            if (source.ParentTypeCode == "SUBSTATION")
                if (String.IsNullOrEmpty(source.ParentName) == false)
                    name = source.ParentName + " - " + name;
            this.ResultName = name;

        }

        public bool Processed
        {
            get { return Status == ListPointStatus.Готово; }
        }

        public string ResultName
        {
            get { return _resultName; }
            set { SetProperty(ref _resultName, value); }
        }

        public string ResultType
        {
            get { return _resultType; }
            set { SetProperty(ref _resultType, value); }
        }

        public object ResultValue
        {
            get { return _resultValue; }
            set { SetProperty(ref _resultValue, value); }
        }

        /// <summary>
        /// Processed | Wait
        /// </summary>
        public ListPointStatus Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);
                OnPropertyChanged("Processed");
            }
        }
    }
}
