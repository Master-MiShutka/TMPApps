using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;

namespace TMP.Wpf.Common.Controls.TableView
{
    public interface ITableViewColumnHeader
    {
        int ColumnIndex { get; }
        TableViewColumn Column { get; }
    }

    public class TableViewColumnHeader : ButtonBase, ITableViewColumnHeader
    {
        public int ColumnIndex { get { return Column.ColumnIndex; } }
        public TableViewColumn Column { get { return this.Content as TableViewColumn; } }

        internal void AdjustWidth(double width)
        {
            if (width < 1)
                width = 1;

            Width = width;  // adjust the width of this control

            Column.AdjustWidth(width);  // adjust the width of the column
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var col = this.Content as TableViewColumn;
            if (col != null)
            {
                this.ContentTemplate = col.TitleTemplate;

                this.Tag = col.Tag;

                if (col.UseHistogramm)
                {
                    if (ContextMenu == null)
                        ContextMenu = new System.Windows.Controls.ContextMenu();
                    System.Windows.Controls.MenuItem mi1 = new System.Windows.Controls.MenuItem();
                    mi1.Header = "Показать гистограмму";
                    mi1.IsCheckable = true;
                    mi1.IsChecked = col.ShowHistogramm;
                    mi1.Command = new DelegateCommand<TableViewColumn>(c =>
                    {
                        c.ShowHistogramm = !c.ShowHistogramm;
                    });
                    mi1.CommandParameter = col;
                    Binding b = new Binding("ShowHistogramm");
                    BindingOperations.SetBinding(mi1, ToggleButton.IsCheckedProperty, b);
                    ContextMenu.Items.Add(mi1);
                }

                if (col.ContextMenu != null && col.ContextMenu.Items != null)
                {
                    if (ContextMenu == null)
                        ContextMenu = new System.Windows.Controls.ContextMenu();
                    foreach (System.Windows.Controls.MenuItem item in col.ContextMenu.Items)
                    {
                        System.Windows.Controls.MenuItem mi2 = new System.Windows.Controls.MenuItem();
                        mi2.Header = item.Header;
                        mi2.Command = item.Command;
                        mi2.CommandParameter = item.CommandParameter;
                        mi2.Tag = item.Tag;
                        ContextMenu.Items.Add(mi2);
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Focus();
            Column.FocusColumn();
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Focus();
            Column.FocusColumn();
            base.OnMouseRightButtonDown(e);
        }
    }
}