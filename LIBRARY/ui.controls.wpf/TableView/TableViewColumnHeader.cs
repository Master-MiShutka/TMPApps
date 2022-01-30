namespace TMP.UI.Controls.WPF.TableView
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;

    public interface ITableViewColumnHeader
    {
        int ColumnIndex { get; }

        TableViewColumn Column { get; }
    }

    public class TableViewColumnHeader : ButtonBase, ITableViewColumnHeader
    {
        public int ColumnIndex => this.Column.ColumnIndex;

        public TableViewColumn Column => this.Content as TableViewColumn;

        internal void AdjustWidth(double width)
        {
            if (width < 1)
            {
                width = 1;
            }

            this.Width = width;  // adjust the width of this control

            this.Column.AdjustWidth(width);  // adjust the width of the column
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
                    if (this.ContextMenu == null)
                    {
                        this.ContextMenu = new System.Windows.Controls.ContextMenu();
                    }

                    System.Windows.Controls.MenuItem mi1 = new System.Windows.Controls.MenuItem();
                    mi1.Header = "Показать гистограмму";
                    mi1.IsCheckable = true;
                    mi1.IsChecked = col.ShowHistogramm;
                    mi1.Command = new Shared.Commands.DelegateCommand<TableViewColumn>(c =>
                    {
                        TableViewColumn tableViewColumn = c;
                        tableViewColumn.ShowHistogramm = !tableViewColumn.ShowHistogramm;
                    });
                    mi1.CommandParameter = col;
                    Binding b = new Binding("ShowHistogramm");
                    BindingOperations.SetBinding(mi1, ToggleButton.IsCheckedProperty, b);
                    this.ContextMenu.Items.Add(mi1);
                }

                if (col.ContextMenu != null && col.ContextMenu.Items != null)
                {
                    if (this.ContextMenu == null)
                    {
                        this.ContextMenu = new System.Windows.Controls.ContextMenu();
                    }

                    foreach (System.Windows.Controls.MenuItem item in col.ContextMenu.Items)
                    {
                        System.Windows.Controls.MenuItem mi2 = new System.Windows.Controls.MenuItem();
                        mi2.Header = item.Header;
                        mi2.Command = item.Command;
                        mi2.CommandParameter = item.CommandParameter;
                        mi2.Tag = item.Tag;
                        this.ContextMenu.Items.Add(mi2);
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.Focus();
            this.Column.FocusColumn();
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            this.Focus();
            this.Column.FocusColumn();
            base.OnMouseRightButtonDown(e);
        }
    }
}
