namespace TMP.UI.Controls.WPF
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// TreeView wich elements HorizontalAlingment is stretch
    /// </summary>
    public class StretchingTreeView : TreeView
    {
        static StretchingTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StretchingTreeView), new FrameworkPropertyMetadata(typeof(StretchingTreeView)));
        }

        public StretchingTreeView() { }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem(this);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            this.SelectedTreeItem = e.NewValue;
        }

        /// <summary>
        /// Selected element
        /// </summary>
        public object SelectedTreeItem
        {
            get { return (object)this.GetValue(SelectedTreeItemProperty); }
            set { this.SetValue(SelectedTreeItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedTreeItemProperty =
            DependencyProperty.Register(nameof(SelectedTreeItem), typeof(object), typeof(StretchingTreeView),
                new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
