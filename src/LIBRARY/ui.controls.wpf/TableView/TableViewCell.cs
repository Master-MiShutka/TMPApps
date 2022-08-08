namespace TMP.UI.WPF.Controls.TableView
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;

    //------------------------------------------------------------------
    public class TableViewCell : ContentControl
    {
        public static readonly DependencyPropertyKey IsSelectedPropertyKey =
              DependencyProperty.RegisterReadOnly("IsSelected", typeof(bool), typeof(TableViewCell), new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        private static void OnIsSelectedChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                (source as TableViewCell).UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            if (this.ParentTableView.SelectedCell != null)
            {
                this.ParentTableView.SelectedCell.IsSelected = false;
            }

            this.ParentTableView.SelectedCell = this;
        }

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            private set => this.SetValue(IsSelectedPropertyKey, value);
        }

        // ******************************************************************
        public static readonly DependencyProperty ParentColumnProperty =
                    DependencyProperty.Register("ParentColumn", typeof(TableViewColumn), typeof(TableViewCell), new PropertyMetadata(null));

        public TableViewColumn ParentColumn
        {
            get => (TableViewColumn)this.GetValue(ParentColumnProperty);

            private set
            {
                if (this.ParentColumn != null)
                {
                    this.ParentColumn.PropertyChanged -= this.ParentColumn_PropertyChanged;
                }

                this.SetValue(ParentColumnProperty, value);
                if (value != null)
                {
                    this.ParentColumn.PropertyChanged += this.ParentColumn_PropertyChanged;
                }
            }
        }

        private void ParentColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        // ******************************************************************
        private TableViewCellsPresenter ParentCellsPresenter = null;

        public TableView ParentTableView = null;

        public int ColumnIndex => this.ParentTableView.Columns.IndexOf(this.ParentColumn);

        public object Item => this.ParentCellsPresenter.Item;

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var tmp = new Size(Math.Max(0.0, arrangeBounds.Width - 1), arrangeBounds.Height);
            Size sz = base.ArrangeOverride(tmp);
            sz.Width += 1;
            return sz;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var tmp = new Size(Math.Max(0.0, constraint.Width - 1), constraint.Height);
            Size sz = base.MeasureOverride(tmp);
            sz.Width += 1;
            return sz;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double verticalGridLineThickness = 1;
            Rect rectangle = new Rect(new Size(verticalGridLineThickness, this.RenderSize.Height));
            rectangle.X = this.RenderSize.Width - verticalGridLineThickness;
            drawingContext.DrawRectangle(this.ParentTableView.GridLinesBrush, null, rectangle);

            if (this.ParentTableView != null && this.ParentColumn != null && this.ParentColumn.ShowHistogramm && this.Content != null)
            {
                double? value = this.DataContext as Nullable<double>;
                if (value.HasValue && value.Value != 0.0)
                {
                    TableViewColumnTotal total = this.ParentColumn.TotalInfo;
                    if (total != null)
                    {
                        double margin = 2.0;
                        double width = this.RenderSize.Width - (2.0 * margin);
                        width = width < 0.0 ? 0.0 : width;
                        double height = this.RenderSize.Height - margin;
                        height = height < 0.0 ? 0.0 : height;

                        // отрисовка вертикальной оси, когда имеются отрицательные значения
                        double axisXPos = total.NullRealative * width;
                        if (total.MinOfValues.HasValue && total.MinOfValues.Value < 0)
                        {
                            Pen dashed_pen = new Pen(this.ParentTableView.HistogramAxisBrush, this.ParentTableView.HistogramAxisThickness);
                            dashed_pen.DashStyle = DashStyles.Dash;
                            drawingContext.DrawLine(dashed_pen, new Point(axisXPos, margin / 2.0), new Point(axisXPos, height));
                        }

                        // отрисовка прямоугольника
                        Pen histogramBarBorderPen = new Pen(this.ParentTableView.HistogramBarBorderBrush, this.ParentTableView.HistogramBarBorderThickness);
                        double barWidth = Math.Abs(value.Value) / total.AbsMinPlusMax * width;
                        if (value.Value < 0)
                        {
                            rectangle = new Rect(new Size(barWidth, height));
                            rectangle.X = axisXPos - barWidth + margin;
                            rectangle.Y = margin / 2.0;
                            drawingContext.DrawRectangle(this.ParentTableView.HistogramNegativeBarFillBrush, histogramBarBorderPen, rectangle);
                        }
                        else
                        {
                            rectangle = new Rect(new Size(barWidth, height));
                            rectangle.X = margin + axisXPos;
                            rectangle.Y = margin / 2.0;
                            drawingContext.DrawRectangle(this.ParentTableView.HistogramPositiveBarFillBrush, histogramBarBorderPen, rectangle);
                        }
                    }
                }
            }
        }

        public void PrepareCell(TableViewCellsPresenter parent, int idx)
        {
            this.ParentCellsPresenter = parent;
            this.ParentTableView = parent.ParentTableView;

            var column = this.ParentTableView.Columns[idx];

            // IsSelected = ParentCellsPresenter.IsSelected() && (ParentTableView.FocusedColumnIndex == column.ColumnIndex);
            if (this.ParentColumn != column)
            {
                this.ParentColumn = column;
                this.Width = column.Width;
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.SetBinding(this, WidthProperty, column.WidthBinding);
                this.Focusable = this.ParentTableView.CellNavigation;
            }

            column.GenerateCellContent(this);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (this.ParentTableView.CellNavigation)
            {
                this.ParentColumn.FocusColumn();
            }

            this.IsSelected = true;

            // ParentTableView.FocusedItemChanged(ParentCellsPresenter);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.ParentColumn.FocusColumn();
            this.Focus();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            this.ParentColumn.FocusColumn();
            this.Focus();
        }
    }
}