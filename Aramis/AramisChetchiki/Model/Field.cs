namespace TMP.WORK.AramisChetchiki.Model
{
    using System;

    [Serializable]
    public class Field : Shared.PropertyChangedBase
    {
        private string header;
        private string fieldName;
        private bool isChecked = true;
        private bool isVisible;
        private int displayIndex;
        private System.ComponentModel.ListSortDirection sortDirection = System.ComponentModel.ListSortDirection.Ascending;

        public Field()
        {
        }

        public Field(string fieldName)
        {
            this.fieldName = fieldName;
        }

        #region Properties

        public string Header
        {
            get => this.header;
            set => this.SetProperty(ref this.header, value);
        }

        public string FieldName
        {
            get => this.fieldName;
            set => this.SetProperty(ref this.fieldName, value);
        }

        public bool IsChecked
        {
            get => this.isChecked;
            set => this.SetProperty(ref this.isChecked, value);
        }

        public bool IsVisible
        {
            get => this.isVisible;
            set => this.SetProperty(ref this.isVisible, value);
        }

        public int DisplayIndex
        {
            get => this.displayIndex;
            set => this.SetProperty(ref this.displayIndex, value);
        }

        public System.ComponentModel.ListSortDirection SortDirection
        {
            get => this.sortDirection;
            set => this.SetProperty(ref this.sortDirection, value);
        }

        #endregion
    }
}
