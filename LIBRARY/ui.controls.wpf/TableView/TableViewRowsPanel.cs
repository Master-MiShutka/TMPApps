namespace TMP.UI.Controls.WPF.TableView
{
    using System.Windows;
    using System.Windows.Controls;

    public class TableViewRowsPanel : VirtualizingStackPanel
    {
        private TableView parentTableView;

        private TableView ParentTableView
        {
            get
            {
                if (this.parentTableView == null)
                {
                    this.parentTableView = TableViewUtils.FindParent<TableView>(this);
                }

                return this.parentTableView;
            }
        }

        private TableViewRowsPresenter parentRowsPresenter;

        private TableViewRowsPresenter ParentRowsPresenter
        {
            get
            {
                if (this.parentRowsPresenter == null)
                {
                    this.parentRowsPresenter = TableViewUtils.FindParent<TableViewRowsPresenter>(this);
                }

                return this.parentRowsPresenter;
            }
        }

        protected override void OnViewportOffsetChanged(Vector oldViewportOffset, Vector newViewportOffset)
        {
            this.ParentTableView.HorizontalScrollOffset = newViewportOffset.X;
        }

        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);
            this.Style = this.ParentTableView.RowsPanelStyle;
            this.ParentRowsPresenter.RowsPanel = this;
        }

        public void BringRowIntoView(int idx)
        {
            if (idx >= 0 && idx < this.ParentRowsPresenter.Items.Count)
            {
                this.BringIndexIntoView(idx);
            }
        }

        internal void ColumnsChanged()
        {
            foreach (var child in this.Children)
            {
                (child as TableViewCellsPresenter).ColumnsChanged();
            }
        }

        internal void RowsInvalidateArrange()
        {
            foreach (var child in this.Children)
            {
                (child as TableViewCellsPresenter).CellsInvalidateArrange();
            }
        }
    }
}
