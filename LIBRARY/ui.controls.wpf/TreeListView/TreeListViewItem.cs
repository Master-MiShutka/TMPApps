namespace TMP.UI.Controls.WPF.TreeListView
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class TreeListViewItem : ListViewItem, INotifyPropertyChanged
    {
        #region Properties

        private TreeNode node;

        public TreeNode Node
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

        public TreeListViewItem()
        {
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

        private void ChangeFocus(TreeNode node)
        {
            var tree = node.Tree;
            if (tree != null)
            {
                var item = tree.ItemContainerGenerator.ContainerFromItem(node) as TreeListViewItem;
                if (item != null)
                {
                    item.Focus();
                }
                else
                {
                    tree.PendingFocusNode = node;
                }
            }
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
