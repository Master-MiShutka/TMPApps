namespace TMP.UI.WPF.Controls.Behaviours
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Microsoft.Xaml.Behaviors;

    public class SelectAllBehavior : Behavior<ListBox>
    {
        private bool isListBoxUpdating;

        public bool? AreAllSelected
        {
            get => (bool?)this.GetValue(AreAllSelectedProperty);
            set => this.SetValue(AreAllSelectedProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="AreAllSelected"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AreAllSelectedProperty =
            DependencyProperty.Register("AreAllSelected", typeof(bool?), typeof(SelectAllBehavior),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, e) => ((SelectAllBehavior)sender).AreAllFilesSelected_Changed((bool?)e.NewValue)));

        protected override void OnAttached()
        {
            base.OnAttached();

            var listBox = this.AssociatedObject;
            if (listBox == null)
            {
                return;
            }

            listBox.SelectAll();

            listBox.SelectionChanged += this.ListBox_SelectionChanged;
            ((INotifyCollectionChanged)listBox.Items).CollectionChanged += (_, __) => this.ListBox_CollectionChanged();
        }

        private void ListBox_SelectionChanged(object? sender, EventArgs e)
        {
            var listBox = this.AssociatedObject;
            if (listBox == null)
            {
                return;
            }

            try
            {
                this.isListBoxUpdating = true;

                if (listBox.Items.Count == listBox.SelectedItems.Count)
                {
                    this.AreAllSelected = true;
                }
                else if (listBox.SelectedItems.Count == 0)
                {
                    this.AreAllSelected = false;
                }
                else
                {
                    this.AreAllSelected = null;
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

            if (!this.AreAllSelected.GetValueOrDefault())
            {
                return;
            }

            listBox?.SelectAll();
        }

        private void AreAllFilesSelected_Changed(bool? newValue)
        {
            var listBox = this.AssociatedObject;
            if (listBox == null)
            {
                return;
            }

            if (this.isListBoxUpdating)
            {
                return;
            }

            if (newValue == null)
            {
                this.Dispatcher?.BeginInvoke(() => this.AreAllSelected = false);
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
