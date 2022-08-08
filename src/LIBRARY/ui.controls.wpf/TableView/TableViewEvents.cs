namespace TMP.UI.WPF.Controls.TableView
{
    using System;

    public class TableViewColumnEventArgs : EventArgs
    {
        public TableViewColumnEventArgs(TableViewColumn column)
        {
            this.Column = column;
        }

        public TableViewColumn Column { get; private set; }
    }
}
