namespace TMP.UI.WPF.Controls.TableView
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    public class TableViewRowsPresenter : ItemsControl
    {
        public TableViewRowsPanel RowsPanel { get; set; }

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

        internal object GetItemAtLocation(Point loc)
        {
            var uie = this.InputHitTest(loc) as FrameworkElement;

            if (uie != null)
            {
                var rowPresenter = TableViewUtils.GetAncestorByType<TableViewCellsPresenter>(uie);

                if (rowPresenter != null)
                {
                    return rowPresenter.Item;
                }
            }

            return null;
        }

        internal int GetCellIndexAtLocation(Point loc)
        {
            var uie = this.InputHitTest(loc) as FrameworkElement;
            if (uie != null)
            {
                var rowPresenter = TableViewUtils.GetAncestorByType<TableViewCellsPresenter>(uie);

                if (rowPresenter != null)
                {
                    var cell = TableViewUtils.FindParent<TableViewCell>(uie);
                    if (cell != null)
                    {
                        return rowPresenter.ItemContainerGenerator.IndexFromContainer(cell);
                    }
                }
            }

            return -1;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.ItemsSource = this.ParentTableView.ItemsSource;
        }

        protected override bool HandlesScrolling => true;

        public void ColumnsChanged()
        {
            if (this.RowsPanel != null)
            {
                this.RowsPanel.ColumnsChanged();
            }
        }

        public void RowsInvalidateArrange()
        {
            if (this.RowsPanel != null)
            {
                this.RowsPanel.RowsInvalidateArrange();
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            this.BringIndexIntoView(this.ParentTableView.FocusedRowIndex);
        }

        // Container generator
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            (element as TableViewCellsPresenter).Clear();
            base.ClearContainerForItemOverride(element, item);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            (element as TableViewCellsPresenter).PrepareRow(this.ParentTableView, item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var container = new TableViewCellsPresenter();
            return container;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TableViewCellsPresenter;
        }

        private int focusedRowIndex;

        private void OnStatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                this.ItemContainerGenerator.StatusChanged -= this.OnStatusChanged;

                var el2 = this.ItemContainerGenerator.ContainerFromIndex(this.focusedRowIndex) as FrameworkElement;
                if (el2 != null)
                {
                    el2.Focus();
                    el2.BringIntoView();
                }
            }
        }

        protected void BringIndexIntoView(int idx)
        {
            if (this.RowsPanel != null && idx >= 0 && idx < this.Items.Count)
            {
                this.focusedRowIndex = idx;
                var el = this.ItemContainerGenerator.ContainerFromIndex(this.focusedRowIndex) as FrameworkElement;
                if (el == null)
                {
                    this.ItemContainerGenerator.StatusChanged += this.OnStatusChanged;
                    this.RowsPanel.BringRowIntoView(this.focusedRowIndex);
                }
                else
                  if (el != null)
                {
                    el.Focus();
                    el.BringIntoView();
                }
            }
        }

        protected void OnPageUpOrDownKeyDown(KeyEventArgs e)
        {
            bool moveForward = e.Key == Key.Next;

            int idx = this.ItemContainerGenerator.IndexFromContainer(e.OriginalSource as FrameworkElement);

            int viewheight = (this.RowsPanel != null) ? (int)this.RowsPanel.ViewportHeight : 0;
            idx = moveForward ? idx + viewheight : idx - viewheight; // calculate the new index for the focus

            if (idx < 0 || idx >= this.Items.Count) // adjust to the bounds of the collection
            {
                idx = moveForward ? this.Items.Count - 1 : 0;
            }

            this.BringIndexIntoView(idx);
            e.Handled = true;
        }

        protected void OnHomeOrEndKeyDown(KeyEventArgs e)
        {
            int idx = (e.Key == Key.Home) ? 0 : this.Items.Count - 1;
            this.BringIndexIntoView(idx);
            e.Handled = true;
        }

        protected void OnLeftOrRightKeyDown(KeyEventArgs e)
        {
            if (this.ParentTableView.CellNavigation == false)
            {
                e.Handled = true;
            }
            else
            {
                if (e.Key == Key.Left && this.ParentTableView.FocusedColumnIndex <= 0)
                {
                    e.Handled = true;
                }

                if (e.Key == Key.Right && this.ParentTableView.FocusedColumnIndex >= this.ParentTableView.Columns.Count - 1)
                {
                    e.Handled = true;
                }
            }
        }

        protected void OnUpOrDownKeyDown(KeyEventArgs e)
        {
            int idx = this.ItemContainerGenerator.IndexFromContainer(e.OriginalSource as FrameworkElement);

            // make sure that we are within the collection
            if (e.Key == Key.Up && idx <= 0)
            {
                e.Handled = true;
            }

            if (e.Key == Key.Down && idx >= this.Items.Count - 1)
            {
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Key key = e.Key;
            switch (key)
            {
                case Key.Prior:
                case Key.Next:
                    this.OnPageUpOrDownKeyDown(e);
                    break;

                case Key.End:
                case Key.Home:
                    this.OnHomeOrEndKeyDown(e);
                    break;

                case Key.Left:
                case Key.Right:
                    this.OnLeftOrRightKeyDown(e);
                    break;

                case Key.Up:
                case Key.Down:
                    this.OnUpOrDownKeyDown(e);
                    break;
            }

            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            // if we have clicked on the control but not on a row.
            if (this.GetItemAtLocation(e.GetPosition(this)) == null)
            {
                this.Focus();
            }

            base.OnMouseDown(e);
        }
    }
}
