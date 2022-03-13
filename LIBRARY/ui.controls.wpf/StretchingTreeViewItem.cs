namespace TMP.UI.Controls.WPF
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    public class StretchingTreeViewItem : TreeViewItem
    {
        public StretchingTreeViewItem()
        {
            this.Loaded += new RoutedEventHandler(this.StretchingTreeViewItem_Loaded);
        }

        private void StretchingTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.VisualChildrenCount > 0)
            {
                if (this.GetVisualChild(0) is Grid grid && grid.ColumnDefinitions.Count == 3)
                {
                    grid.ColumnDefinitions.RemoveAt(2);
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }

        /// <summary>
        /// True is it's model property contains <see cref="StretchingTreeView.SearchString"/>
        /// <seealso cref="StretchingTreeView.SearchMemberPath"/>
        /// </summary>
        public bool IsMatch
        {
            get { return (bool)this.GetValue(IsMatchProperty); }
            set { this.SetValue(IsMatchProperty, value); }
        }

        public static readonly DependencyProperty IsMatchProperty =
            DependencyProperty.Register(nameof(IsMatch), typeof(bool), typeof(StretchingTreeViewItem), new PropertyMetadata(true));
    }
}
