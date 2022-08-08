namespace TMP.UI.WPF.Controls.TableView
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    public class TableViewCellsPresenter : ItemsControl
    {
        #region Dependency Properties

        public static readonly DependencyPropertyKey IsSelectedPropertyKey =
          DependencyProperty.RegisterReadOnly("IsSelected", typeof(bool), typeof(TableViewCellsPresenter),
              new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        private static void OnIsSelectedChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                (source as TableViewCellsPresenter).UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            if (this.ParentTableView.SelectedCellsPresenter != null)
            {
                this.ParentTableView.SelectedCellsPresenter.IsSelected = false;
            }

            this.ParentTableView.SelectedCellsPresenter = this;
        }

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            private set => this.SetValue(IsSelectedPropertyKey, value);
        }

        //----------------------------
        public static readonly DependencyProperty IsMarkedProperty =
          DependencyProperty.Register("IsMarked", typeof(bool), typeof(TableViewCellsPresenter),
              new FrameworkPropertyMetadata(false));

        public bool IsMarked
        {
            get => (bool)this.GetValue(IsMarkedProperty);
            set => this.SetValue(IsMarkedProperty, value);
        }
        #endregion Dependency Properties

        public TableView ParentTableView { get; set; }

        public TableViewCellsPanel CellsPanel { get; set; }

        protected override bool HandlesScrolling => true;

        public object Item
        {
            get => this.ItemsSource == null ? null : (this.ItemsSource as TableViewCellCollection).CopyObject;

            private set
            {
                if (this.ItemsSource == null)
                {
                    this.ItemsSource = new TableViewCellCollection(value, this.ParentTableView.Columns.Count);
                }
                else
                {
                    (this.ItemsSource as TableViewCellCollection).CopyObject = value;
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (this.ParentTableView.ShowHorizontalGridLines)
            {
                var pen = new Pen(this.ParentTableView.GridLinesBrush, 1.0);
                drawingContext.DrawLine(pen, new Point(1, this.RenderSize.Height - 0.5), new Point(this.RenderSize.Width, this.RenderSize.Height - 0.5));

                // Rect rectangle = new Rect(new Size(base.RenderSize.Width, 1));
                // rectangle.Y = base.RenderSize.Height - 1;
                // drawingContext.DrawRectangle(ParentTableView.GridLinesBrush, null, rectangle);
            }
        }

        public void ColumnsChanged()
        {
            var item = this.Item;
            this.ItemsSource = null;
            this.Item = item;

            this.CellsInvalidateArrange();
        }

        public void UpdateColumns()
        {
            (this.ItemsSource as TableViewCellCollection).Count = this.ParentTableView.Columns.Count;
        }

        public void CellsInvalidateArrange()
        {
            this.UpdateColumns();

            if (this.CellsPanel != null)
            {
                this.CellsPanel.InvalidateArrange();
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (this.ParentTableView.ShowHorizontalGridLines)
            {
                var tmp = new Size(arrangeBounds.Width, arrangeBounds.Height - 1);
                var size = base.ArrangeOverride(tmp);
                size.Height += 1;
                return size;
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (this.ParentTableView.ShowHorizontalGridLines)
            {
                Size tmp = constraint;

                if (tmp.Height > 0)
                {
                    tmp = new Size(constraint.Width, constraint.Height - 1);
                }

                var size = base.MeasureOverride(tmp);
                size.Height += 1;
                return size;
            }

            return base.MeasureOverride(constraint);
        }

        public void PrepareRow(TableView parent, object dataItem)
        {
            this.ParentTableView = parent;

            this.Focusable = this.ParentTableView.CellNavigation == false;

            this.Item = dataItem;

            // set the selected state for this row
            var scp = this.ParentTableView.SelectedCellsPresenter;
            if (scp != null)
            {
                this.IsSelected = this.ParentTableView.IndexOfRow(scp) == this.ParentTableView.IndexOfRow(this);
            }
        }

        public void Clear()
        {
            this.Item = null;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            (element as TableViewCell).PrepareCell(this, this.ItemContainerGenerator.IndexFromContainer(element));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TableViewCell();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TableViewCell;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            this.ParentTableView.FocusedRowChanged(this);
            this.IsSelected = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.ParentTableView.FocusedRowChanged(this);
            base.OnMouseLeftButtonDown(e);
            this.Focus();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            this.IsMarked = !this.IsMarked;
            this.ParentTableView.FocusedRowChanged(this);
            base.OnMouseRightButtonDown(e);

            // this.Focus();
        }
    }
}