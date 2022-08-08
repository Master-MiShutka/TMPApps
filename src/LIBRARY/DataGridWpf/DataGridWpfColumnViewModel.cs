namespace DataGridWpf
{
    using System;
    using System.ComponentModel;

    [Serializable]
    [TypeConverter(typeof(DataGridWpfColumnConverter))]
    [System.Diagnostics.DebuggerDisplay("Field='{FieldName}', Title='{Title}', DisplayIndex={DisplayIndex}, {IsVisible}")]
    public class DataGridWpfColumnViewModel : TMP.Shared.Common.PropertyChangedBase
    {
        public const string DefaultContentExportFormat = "@";

        private string cellContentExportStringFormat;
        private Type dataType;
        private string fieldName;
        private string title;
        private bool isVisible;
        private int displayIndex;
        private string cellContentStringFormat;
        private string groupName;
        private object tag;

        public Type DataType
        {
            get => this.dataType; set
            {
                if (this.SetProperty(ref this.dataType, value))
                {
                    this.RaisePropertyChanged(nameof(this.CellContentExportStringFormat));
                }
            }
        }

        public string FieldName { get => this.fieldName; set => this.SetProperty(ref this.fieldName, value); }

        public string Title { get => this.title; set => this.SetProperty(ref this.title, value); }

        public bool IsVisible { get => this.isVisible; set => this.SetProperty(ref this.isVisible, value); }

        public int DisplayIndex { get => this.displayIndex; set => this.SetProperty(ref this.displayIndex, value); }

        public string CellContentStringFormat { get => this.cellContentStringFormat; set => this.SetProperty(ref this.cellContentStringFormat, value); }

        public string CellContentExportStringFormat
        {
            get => string.IsNullOrEmpty(this.cellContentExportStringFormat)
                    ? this.DataType switch
                    {
                        Type t when t == typeof(bool) => string.Empty,
                        Type t when t == typeof(string) => DefaultContentExportFormat,
                        Type t when t == typeof(int) => @"# ##0_ ;[Красный]-# ##0\ ",
                        Type t when t == typeof(double) => @"# ##0_ ;[Красный]-# ##0\ ",
                        Type t when t == typeof(DateTime) => @"[$-ru-BY-x-genlower]dddd, d mmmm yyyy",
                        _ => DefaultContentExportFormat
                    }
                    : this.cellContentExportStringFormat;
            set => this.cellContentExportStringFormat = value;
        }

        public string GroupName { get => this.groupName; set => this.SetProperty(ref this.groupName, value); }

        public object Tag { get => this.tag; set => this.SetProperty(ref this.tag, value); }

        internal void SetIsVisible(bool value)
        {
            this.isVisible = value;
            this.RaisePropertyChanged(nameof(this.IsVisible));
        }

        internal void SetDisplayIndex(int value)
        {
            this.displayIndex = value;
            this.RaisePropertyChanged(nameof(this.DisplayIndex));
        }
    }
}
