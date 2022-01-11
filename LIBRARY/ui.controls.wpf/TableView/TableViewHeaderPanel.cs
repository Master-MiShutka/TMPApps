namespace TMP.UI.Controls.WPF.TableView
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class TableViewHeaderPanel : Panel
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

        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);

            this.Style = this.ParentTableView.HeaderPanelStyle;

            this.ParentTableView.HeaderRowPresenter.HeaderItemsPanel = this;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var columns = this.ParentTableView.Columns;
            var children = this.Children;
            double leftX = 0;
            int fixedColumnCount = this.ParentTableView.FixedColumnCount;

            this.ParentTableView.ResetFixedClipRect();

            Rect fixedClip = this.ParentTableView.FixedClipRect;
            fixedClip.X = 0;
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

            return arrangeSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = new Size();

            var children = this.Children;
            foreach (var child in children)
            {
                var element = child as UIElement;
                element.Measure(availableSize);
                size.Width += element.DesiredSize.Width;
                size.Height = Math.Max(size.Height, element.DesiredSize.Height);
            }

            return size;
        }
    }
}