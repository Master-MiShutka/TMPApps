namespace TMP.UI.WPF.Controls.Behaviours
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Xaml.Behaviors;

    public class SelectAllColumnsBehavior : Behavior<CheckBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Checked += this.CheckBox_Checked;
            this.AssociatedObject.Unchecked += this.CheckBox_Unchecked;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.Checked -= this.CheckBox_Checked;
            this.AssociatedObject.Unchecked -= this.CheckBox_Unchecked;

            var listBox = this.ListBox;
            if (listBox != null)
            {
                listBox.SelectionChanged -= this.ListBox_SelectionChanged;
            }
        }

        public ListBox ListBox
        {
            get => (ListBox)this.GetValue(ListBoxProperty);
            set => this.SetValue(ListBoxProperty, value);
        }

        public static readonly DependencyProperty ListBoxProperty =
            DependencyProperty.Register("ListBox", typeof(ListBox), typeof(SelectAllColumnsBehavior), new FrameworkPropertyMetadata(default, (sender, e) => ((SelectAllColumnsBehavior)sender).ListBox_Changed((ListBox)e.OldValue, (ListBox)e.NewValue)));

        private void ListBox_Changed(ListBox? oldValue, ListBox? newValue)
        {
            if (oldValue != null)
            {
                oldValue.SelectionChanged -= this.ListBox_SelectionChanged;
            }

            if (newValue != null)
            {
                newValue.SelectionChanged += this.ListBox_SelectionChanged;
            }
        }

        private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
            {
                return;
            }

            var items = listBox.Items.OfType<DataGridColumn>().ToList();

            var visibleItemsCount = items.Count(item => item.Visibility == Visibility.Visible);

            if (visibleItemsCount == 0)
            {
                this.AssociatedObject.IsChecked = false;
            }
            else if (visibleItemsCount == items.Count)
            {
                this.AssociatedObject.IsChecked = true;
            }
            else
            {
                this.AssociatedObject.IsChecked = null;
            }
        }

        private void CheckBox_Unchecked(object? sender, RoutedEventArgs e)
        {
            var listBox = this.ListBox;
            if (listBox == null)
            {
                return;
            }

            var itemsToUnselect = listBox.SelectedItems;

            foreach (var item in itemsToUnselect)
            {
                listBox.SelectedItems.Remove(item);
            }
        }

        private void CheckBox_Checked(object? sender, RoutedEventArgs e)
        {
            var listBox = this.ListBox;
            if (listBox == null)
            {
                return;
            }

            var selectedItems = listBox.SelectedItems.OfType<DataGridColumn>().ToList();
            var allItems = listBox.Items.OfType<DataGridColumn>().ToList();
            var itemsToSelect = allItems.Except(selectedItems);

            foreach (var item in itemsToSelect)
            {
                listBox.SelectedItems.Add(item);
            }
        }
    }
}
