namespace TMP.UI.Controls.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    /// <summary>
    /// TreeView wich elements HorizontalAlingment is stretch 
    /// </summary>
    public class StretchingTreeView : TreeView
    {
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

        /// <summary>
        /// Gets a value that indicates whether the tree contains visible items
        /// Returns:
        /// true if the all items Visibility is Visivle; otherwise, false
        /// </summary>
        public bool HasVisibleItems
        {
            get { return (bool)this.GetValue(HasVisibleItemsProperty); }
            private set { this.SetValue(HasVisibleItemsProperty, value); }
        }

        public static readonly DependencyProperty HasVisibleItemsProperty =
            DependencyProperty.Register(nameof(HasVisibleItems), typeof(bool), typeof(StretchingTreeView), new PropertyMetadata(true));

        public string SearchMemberPath
        {
            get { return (string)this.GetValue(SearchMemberPathProperty); }
            set { this.SetValue(SearchMemberPathProperty, value); }
        }

        public static readonly DependencyProperty SearchMemberPathProperty =
            DependencyProperty.Register(nameof(SearchMemberPath), typeof(string), typeof(StretchingTreeView), new FrameworkPropertyMetadata(string.Empty));

        public string SearchString
        {
            get { return (string)this.GetValue(SearchStringProperty); }
            set { this.SetValue(SearchStringProperty, value); }
        }

        public static readonly DependencyProperty SearchStringProperty =
            DependencyProperty.Register(nameof(SearchString), typeof(string), typeof(StretchingTreeView),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchStringChanged));

        private static void OnSearchStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StretchingTreeView treeView = d as StretchingTreeView;
            treeView.ApplyFilter();
        }

        /// <summary>
        /// Loop through all items and set <see cref="StretchingTreeViewItem.IsMatch"/> property if <see cref="SearchStringProperty"/> value contains <see cref="SearchString"/>
        /// </summary>
        private void ApplyFilter()
        {
            if (this.ItemsSource == null)
                return;

            System.Windows.Input.Cursor previousCursor = System.Windows.Input.Mouse.OverrideCursor;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            this.DoApplyFilter(this, this.SearchString);

            this.UpdateLayout();

            if (string.IsNullOrEmpty(this.SearchString))
            {
                for (int i = 0, count = this.Items.Count; i < count; i++)
                {
                    var obj = this.ItemContainerGenerator.ContainerFromIndex(i);

                    if (obj is StretchingTreeViewItem item && item != null && item.IsMatch && item.Visibility == Visibility.Visible)
                    {
                        //item.IsMatch = true;
                    }
                }
            }

            bool flag = false;
            for (int i = 0, count = this.Items.Count; i < count; i++)
            {
                var obj = this.ItemContainerGenerator.ContainerFromIndex(i);

                if (obj is StretchingTreeViewItem item && item != null && item.IsMatch && item.Visibility == Visibility.Visible)
                {
                    flag = true;
                    break;
                }
            }

            this.HasVisibleItems = flag;

            System.Windows.Input.Mouse.OverrideCursor = previousCursor;
        }

        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a StretchingTreeView or a StretchingTreeViewItem.
        /// </param>
        /// <param name="criteria">
        /// The string to search for.
        /// </param>
        private bool DoApplyFilter(ItemsControl container, string criteria)
        {
            bool result = false;
            bool foundAnyMatched = false;

            StretchingTreeViewItem treeViewItem = container as StretchingTreeViewItem;

            if (container != null)
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    return false;
                }

                // Expand the current container
                if (treeViewItem != null && !treeViewItem.IsExpanded)
                {
                    treeViewItem.IsExpanded = true;

                    treeViewItem.ApplyCriteria(criteria);

                    if (treeViewItem.IsMatch == true)
                        result = true;
                }
                else
                {
                    // Try to generate the ItemsPresenter and the ItemsPanel by calling ApplyTemplate.  Note that in the
                    // virtualizing case even if the item is marked expanded we still need to do this step in order to
                    // regenerate the visuals because they may have been virtualized away.
                    container.ApplyTemplate();
                    ItemsPresenter itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                    if (itemsPresenter != null)
                    {
                        itemsPresenter.ApplyTemplate();
                    }
                    else
                    {
                        // The Tree template has not named the ItemsPresenter, so walk the descendents and find the child.
                        itemsPresenter = this.FindVisualChild<ItemsPresenter>(container);
                        if (itemsPresenter == null)
                        {
                            container.UpdateLayout();
                        }
                    }
                }

                while (container.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.NotStarted)
                {
                    container.UpdateLayout();
                }

                bool childResult = false;
                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    DependencyObject dependencyObject = container.ItemContainerGenerator.ContainerFromIndex(i);
                    ItemsControl subContainer = dependencyObject as ItemsControl;
                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        childResult = this.DoApplyFilter(subContainer, criteria);

                        if (foundAnyMatched == false && childResult)
                        {
                            treeViewItem.DoSetIsMatchTrue();
                            foundAnyMatched = true;
                        }
                    }
                }
            }

            return result;
        }

        private static async Task<StretchingTreeViewItem> FindItemContainer(ItemsControl itemsControl, object item)
        {
            ItemContainerGenerator generator = itemsControl.ItemContainerGenerator;
            if (generator.Status != GeneratorStatus.ContainersGenerated)
            {
                var tcs = new TaskCompletionSource<object>();
                void handler(object s, EventArgs e)
                {
                    if (generator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        generator.StatusChanged -= handler;
                        tcs.SetResult(null);
                    }
                    else if (generator.Status == GeneratorStatus.Error)
                    {
                        generator.StatusChanged -= handler;
                        tcs.SetException(new InvalidOperationException());
                    }
                }

                generator.StatusChanged += handler;
                if (itemsControl is StretchingTreeViewItem stvi)
                    stvi.IsExpanded = true;
                itemsControl.UpdateLayout();
                await tcs.Task;
            }

            StretchingTreeViewItem container = (StretchingTreeViewItem)generator.ContainerFromItem(item);
            if (container == null)
            {
                foreach (var parentItem in itemsControl.Items)
                {
                    StretchingTreeViewItem parentContainer = (StretchingTreeViewItem)generator.ContainerFromItem(parentItem);
                    container = await FindItemContainer(parentContainer, item);
                    if (container != null)
                        return container;
                }
            }

            return container;
        }

        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        private T FindVisualChild<T>(Visual visual)
            where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    if (child is T correctlyTyped)
                    {
                        return correctlyTyped;
                    }

                    T descendent = this.FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }
    }
}
