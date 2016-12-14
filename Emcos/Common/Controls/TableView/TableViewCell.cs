using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TMP.Wpf.Common.Controls.TableView
{
    //------------------------------------------------------------------
    public class TableViewCell : ContentControl
    {
        public static readonly DependencyPropertyKey IsSelectedPropertyKey =
              DependencyProperty.RegisterReadOnly("IsSelected", typeof(bool), typeof(TableViewCell), new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        private static void OnIsSelectedChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (source as TableViewCell).UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (ParentTableView.SelectedCell != null)
                ParentTableView.SelectedCell.IsSelected = false;
            ParentTableView.SelectedCell = this;
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            private set { SetValue(IsSelectedPropertyKey, value); }
        }
        //******************************************************************
        public static readonly DependencyProperty ParentColumnProperty =
                    DependencyProperty.Register("ParentColumn", typeof(TableViewColumn), typeof(TableViewCell), new PropertyMetadata(null));

        public TableViewColumn ParentColumn
        {
            get { return (TableViewColumn)GetValue(ParentColumnProperty); }
            private set
            {
                if (ParentColumn != null)
                    ParentColumn.PropertyChanged -= ParentColumn_PropertyChanged;

                SetValue(ParentColumnProperty, value);
                if (value != null)
                    ParentColumn.PropertyChanged += ParentColumn_PropertyChanged;
            }
        }

        private void ParentColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InvalidateVisual();
        }

        //******************************************************************
        private TableViewCellsPresenter ParentCellsPresenter = null;

        public TableView ParentTableView = null;
        public int ColumnIndex { get { return ParentTableView.Columns.IndexOf(ParentColumn); } }
        public object Item { get { return ParentCellsPresenter.Item; } }

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
            Rect rectangle = new Rect(new Size(verticalGridLineThickness, base.RenderSize.Height));
            rectangle.X = base.RenderSize.Width - verticalGridLineThickness;
            drawingContext.DrawRectangle(ParentTableView.GridLinesBrush, null, rectangle);

            if (ParentTableView != null && ParentColumn != null && ParentColumn.ShowHistogramm && Content != null)
            {
                double? value = DataContext as Nullable<double>;
                if (value.HasValue && value.Value != 0.0)
                {
                    TableViewColumnTotal total = ParentColumn.TotalInfo;
                    if (total != null)
                    {
                        double margin = 2.0;
                        double width = base.RenderSize.Width - 2.0 * margin;
                        width = width < 0.0 ? 0.0 : width;
                        double height = base.RenderSize.Height - margin;
                        height = height < 0.0 ? 0.0 : height;

                        // отрисовка вертикальной оси, когда имеются отрицательные значения
                        double axisXPos = total.NullRealative * width;
                        if (total.MinOfValues.HasValue && total.MinOfValues.Value < 0)
                        {
                            Pen dashed_pen = new Pen(ParentTableView.HistogramAxisBrush, ParentTableView.HistogramAxisThickness);
                            dashed_pen.DashStyle = DashStyles.Dash;
                            drawingContext.DrawLine(dashed_pen, new Point(axisXPos, margin / 2.0), new Point(axisXPos, height));
                        }
                        // отрисовка прямоугольника
                        Pen histogramBarBorderPen = new Pen(ParentTableView.HistogramBarBorderBrush, ParentTableView.HistogramBarBorderThickness);
                        double barWidth = Math.Abs(value.Value) / total.AbsMinPlusMax * width;
                        if (value.Value < 0)
                        {
                            rectangle = new Rect(new Size(barWidth, height));
                            rectangle.X = axisXPos - barWidth + margin;
                            rectangle.Y = margin / 2.0;
                            drawingContext.DrawRectangle(ParentTableView.HistogramNegativeBarFillBrush, histogramBarBorderPen, rectangle);
                        }
                        else
                        {
                            rectangle = new Rect(new Size(barWidth, height));
                            rectangle.X = margin + axisXPos;
                            rectangle.Y = margin / 2.0;
                            drawingContext.DrawRectangle(ParentTableView.HistogramPositiveBarFillBrush, histogramBarBorderPen, rectangle);
                        }
                    }
                }
            }
        }

        public void PrepareCell(TableViewCellsPresenter parent, int idx)
        {
            ParentCellsPresenter = parent;
            ParentTableView = parent.ParentTableView;

            var column = ParentTableView.Columns[idx];

            //IsSelected = ParentCellsPresenter.IsSelected() && (ParentTableView.FocusedColumnIndex == column.ColumnIndex);

            if (ParentColumn != column)
            {
                ParentColumn = column;
                this.Width = column.Width;
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.SetBinding(this, WidthProperty, column.WidthBinding);
                Focusable = ParentTableView.CellNavigation;
            }
            column.GenerateCellContent(this);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (ParentTableView.CellNavigation)
                ParentColumn.FocusColumn();
            IsSelected = true;
            //ParentTableView.FocusedItemChanged(ParentCellsPresenter);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            ParentColumn.FocusColumn();
            Focus();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            ParentColumn.FocusColumn();
            Focus();
        }
    }
}