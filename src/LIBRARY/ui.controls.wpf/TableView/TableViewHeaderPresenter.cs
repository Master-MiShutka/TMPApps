namespace TMP.UI.WPF.Controls.TableView
{
    using System.Windows;
    using System.Windows.Controls;

    public class TableViewHeaderPresenter : ItemsControl
    {
        public Panel HeaderItemsPanel { get; set; }

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

        internal TableViewColumnHeader GetColumnHeaderAtLocation(Point loc)
        {
            var uie = this.InputHitTest(loc) as FrameworkElement;
            if (uie != null)
            {
                return TableViewUtils.FindParent<TableViewColumnHeader>(uie);
            }

            return null;
        }

        internal void HeaderInvalidateArrange()
        {
            if (this.HeaderItemsPanel != null)
            {
                this.HeaderItemsPanel.InvalidateArrange();
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            (element as TableViewColumnHeader).Width = (item as TableViewColumn).Width;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TableViewColumnHeader();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TableViewColumnHeader;
        }
    }
}
