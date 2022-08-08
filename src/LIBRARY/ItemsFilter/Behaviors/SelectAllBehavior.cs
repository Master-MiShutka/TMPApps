namespace ItemsFilter.Behaviors
{
    using Microsoft.Xaml.Behaviors;
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    public class SelectAllBehavior : Behavior<ListBox>
    {
        private bool isListBoxUpdating;

        public bool? AreAllFilesSelected
        {
            get => (bool?)this.GetValue(AreAllFilesSelectedProperty);
            set => this.SetValue(AreAllFilesSelectedProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="AreAllFilesSelected"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AreAllFilesSelectedProperty =
            DependencyProperty.Register(
                nameof(AreAllFilesSelected),
                typeof(bool?),
                typeof(SelectAllBehavior),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, e) => ((SelectAllBehavior)sender).OnAreAllFilesSelectedChanged((bool?)e.NewValue)));

        protected override void OnAttached()
        {
            base.OnAttached();

            var listBox = this.AssociatedObject;
            if (listBox == null)
                return;

            listBox.SelectAll();

            listBox.SelectionChanged += this.ListBox_SelectionChanged;
            ((INotifyCollectionChanged)listBox.Items).CollectionChanged += (_, __) => this.ListBox_CollectionChanged();
        }

        private void ListBox_SelectionChanged(object? sender, EventArgs e)
        {
            var listBox = this.AssociatedObject;
            if (listBox == null)
                return;

            try
            {
                this.isListBoxUpdating = true;

                if (listBox.Items.Count == listBox.SelectedItems.Count)
                {
                    this.AreAllFilesSelected = true;
                }
                else if (listBox.SelectedItems.Count == 0)
                {
                    this.AreAllFilesSelected = false;
                }
                else
                {
                    this.AreAllFilesSelected = null;
                }
            }
            finally
            {
                this.isListBoxUpdating = false;
            }
        }

        private void ListBox_CollectionChanged()
        {
            var listBox = this.AssociatedObject;

            if (!this.AreAllFilesSelected.GetValueOrDefault())
                return;

            listBox?.SelectAll();
        }

        private void OnAreAllFilesSelectedChanged(bool? newValue)
        {
            var listBox = this.AssociatedObject;
            if (listBox == null)
                return;

            if (this.isListBoxUpdating)
                return;

            if (newValue == null)
            {
                this.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, () => this.AreAllFilesSelected = false);
                return;
            }

            if (newValue.GetValueOrDefault())
            {
                listBox.SelectAll();
            }
            else
            {
                listBox.SelectedIndex = -1;
            }
        }
    }
}
