namespace TMP.UI.Controls.WPF.TreeListView
{
    using System.Windows;
    using System.Windows.Controls;

    public class TreeListView : ListView
    {
        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
        }

        public TreeListView() { }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem(this);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }
    }
}
