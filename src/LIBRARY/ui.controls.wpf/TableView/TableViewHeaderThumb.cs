namespace TMP.UI.WPF.Controls.TableView
{
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    //-----------------------------------------------------------------------------------
    public class TableViewHeaderThumb : Thumb
    {
        public TableViewHeaderThumb()
          : base()
        {
            this.PreviewMouseLeftButtonDown += (s, e) => Mouse.Capture(this);
            this.PreviewMouseLeftButtonUp += (s, e) => Mouse.Capture(null);
            this.DragDelta += this.onDragDelta;
        }

        public void onDragDelta(object sender, DragDeltaEventArgs e)
        {
            var tvch = TableViewUtils.GetAncestorByType<TableViewColumnHeader>(this);

            if (tvch != null)
            {
                var width = tvch.Width + e.HorizontalChange;
                tvch.AdjustWidth(width);
            }
        }
    }
}
