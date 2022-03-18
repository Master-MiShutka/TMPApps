namespace TMP.UI.Controls.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    public class StretchingTreeViewItem : TreeViewItem
    {
        private StretchingTreeView stretchingTreeView;

        private bool isMatch = true;

        internal StretchingTreeViewItem(StretchingTreeView stretchingTreeView)
        {
            this.stretchingTreeView = stretchingTreeView;

            this.Loaded += new RoutedEventHandler(this.StretchingTreeViewItem_Loaded);
        }

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
            get => (bool)this.GetValue(IsMatchProperty);

            private set => this.SetValue(IsMatchPropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsMatchPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsMatch), typeof(bool), typeof(StretchingTreeViewItem), new PropertyMetadata(true));

        public static readonly DependencyProperty IsMatchProperty = IsMatchPropertyKey.DependencyProperty;

        private string GetSearchPropertyValue()
        {
            string propetyPath = this.stretchingTreeView.SearchMemberPath;
            if (string.IsNullOrEmpty(propetyPath))
            {
                return this.Header.ToString();
            }
            else
            {
                object dc = this.DataContext;
                return dc == null ? string.Empty : (string)dc.GetPropertyValue(propetyPath);
            }
        }

        /// <summary>
        /// Returns True if tree node <see cref="SearchStringProperty"/> value contains <see cref="SearchString"/>
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private bool IsCriteriaMatched(string criteria)
        {
            return string.IsNullOrEmpty(criteria) || this.GetSearchPropertyValue().Contains(criteria, System.StringComparison.Ordinal);
        }

        internal void ApplyCriteria(string criteria)
        {
            this.IsMatch = this.IsCriteriaMatched(criteria);
        }

        internal void DoSetIsMatchTrue()
        {
            this.IsMatch = true;
        }
    }
}
