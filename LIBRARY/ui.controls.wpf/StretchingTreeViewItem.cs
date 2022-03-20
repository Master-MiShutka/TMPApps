namespace TMP.UI.Controls.WPF
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class StretchingTreeViewItem : TreeViewItem
    {
        private StretchingTreeView stretchingTreeView;

        internal StretchingTreeViewItem(StretchingTreeView stretchingTreeView)
        {
            this.stretchingTreeView = stretchingTreeView;

            this.Loaded += new RoutedEventHandler(this.StretchingTreeViewItem_Loaded);
        }

        public StretchingTreeViewItem()
        {
            if (this.stretchingTreeView == null)
            {
                this.stretchingTreeView = FindVisualParent<StretchingTreeView>(this);
            }

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
            return new StretchingTreeViewItem(this.stretchingTreeView);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }

        public static T FindVisualParent<T>(UIElement element)
            where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                if (parent is T correctlyTyped)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }
    }
}
