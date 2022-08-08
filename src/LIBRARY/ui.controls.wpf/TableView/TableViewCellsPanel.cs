namespace TMP.UI.WPF.Controls.TableView
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class TableViewCellsPanel : Panel
    {
        private TableView ParentTableView;

        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);

            var rowPresenter = TableViewUtils.FindParent<TableViewCellsPresenter>(this);

            if (rowPresenter != null)
            {
                rowPresenter.CellsPanel = this;
                this.ParentTableView = rowPresenter.ParentTableView;
                this.Style = this.ParentTableView.CellsPanelStyle;
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.ParentTableView != null)
            {
                var columns = this.ParentTableView.Columns;
                var children = this.Children;
                double leftX = this.ParentTableView.HorizontalScrollOffset;
                int fixedColumnCount = this.ParentTableView.FixedColumnCount;

                Rect fixedClip = this.ParentTableView.FixedClipRect;
                fixedClip.Height = arrangeSize.Height;

                // Arrange the children into a line
                int idx = 0;
                Rect cellRect = new Rect(0, 0, 0, arrangeSize.Height);
                foreach (var child in children)
                {
                    if (idx == fixedColumnCount)
                    {
                        leftX -= this.ParentTableView.HorizontalScrollOffset;
                    }

                    cellRect.X = leftX;
                    cellRect.Width = columns[idx].Width;
                    leftX += cellRect.Width;

                    (child as UIElement).Clip = null;
                    if (idx >= fixedColumnCount)
                    {
                        if (cellRect.Right < fixedClip.Right)
                        {
                            cellRect.X = -cellRect.Width;   // hide children that are to the left of the fixed columns
                        }
                        else
                        {
                            var overlap = fixedClip.Right - cellRect.X; // check for columns that overlap the fixed columns and clip them
                            if (overlap > 0)
                            {
                                var r = new Rect(overlap, cellRect.Y, cellRect.Width - overlap, cellRect.Height);
                                (child as UIElement).Clip = new RectangleGeometry(r);
                            }
                        }
                    }

                    (child as UIElement).Arrange(cellRect);

                    ++idx;
                }
            }

            return arrangeSize;
        }

        private Size size = Size.Empty;

        protected override Size MeasureOverride(Size availableSize)
        {
            this.size = new Size();

            var children = this.Children;
            foreach (var child in children)
            {
                var element = child as UIElement;
                element.Measure(availableSize);
                this.size.Width += element.DesiredSize.Width;
                this.size.Height = Math.Max(this.size.Height, element.DesiredSize.Height);
            }

            // Set a default height for the row if not set by the children
            if (this.size.Height <= 5.0)
            {
                this.size.Height = 15.96;
            }

            return this.size;
        }
    }
}
