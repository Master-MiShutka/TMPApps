﻿namespace TMP.UI.WPF.Controls.TreeListView
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class TreeListViewItem : TreeViewItem, INotifyPropertyChanged
    {
        #region Properties

        private TreeListView treeListView;

        private Shared.Common.Tree.ITreeNode node;

        public Shared.Common.Tree.ITreeNode Node
        {
            get => this.node;

            internal set
            {
                this.node = value;
                this.OnPropertyChanged("Node");
            }
        }

        #endregion

        static TreeListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewItem), new FrameworkPropertyMetadata(typeof(TreeListViewItem)));
        }

        internal TreeListViewItem(TreeListView treeListView)
        {
            this.treeListView = treeListView;
        }

        public TreeListViewItem()
        {
            if (this.treeListView == null)
            {
                this.treeListView = FindVisualParent<TreeListView>(this);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.Node != null)
            {
                switch (e.Key)
                {
                    case Key.Right:
                        e.Handled = true;
                        if (!this.Node.IsExpanded)
                        {
                            this.Node.IsExpanded = true;
                            this.ChangeFocus(this.Node);
                        }
                        else if (this.Node.Children.Count > 0)
                        {
                            this.ChangeFocus(this.Node.Children[0]);
                        }

                        break;

                    case Key.Left:

                        e.Handled = true;
                        if (this.Node.IsExpanded && this.Node.IsExpandable)
                        {
                            this.Node.IsExpanded = false;
                            this.ChangeFocus(this.Node);
                        }
                        else
                        {
                            this.ChangeFocus(this.Node.Parent);
                        }

                        break;

                    case Key.Subtract:
                        e.Handled = true;
                        this.Node.IsExpanded = false;
                        this.ChangeFocus(this.Node);
                        break;

                    case Key.Add:
                        e.Handled = true;
                        this.Node.IsExpanded = true;
                        this.ChangeFocus(this.Node);
                        break;
                }
            }

            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }

        private void ChangeFocus(Shared.Common.Tree.ITreeNode node)
        {
            if (this.treeListView != null)
            {
                if (this.treeListView.ItemContainerGenerator.ContainerFromItem(node) is TreeListViewItem item)
                {
                    item.Focus();
                }
            }
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

                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
