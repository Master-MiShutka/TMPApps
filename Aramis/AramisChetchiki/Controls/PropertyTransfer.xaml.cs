namespace TMP.WORK.AramisChetchiki.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using TMP.Shared;
    using TMP.Shared.Commands;

    /// <summary>
    /// Interaction logic for PropertyTransfer.xaml
    /// </summary>
    public partial class PropertyTransfer : UserControl, INotifyPropertyChanged
    {
        private Predicate<object> existingFilter;
        private int sourceCollectionItemsCount;

        public static readonly DependencyProperty SourceCollectionProperty =
            DependencyProperty.Register(
                nameof(SourceCollection),
                typeof(ICollectionView),
                typeof(PropertyTransfer),
                new PropertyMetadata(default, OnSourceCollectionChanged));

        public static readonly DependencyProperty TargetCollectionProperty =
            DependencyProperty.Register(
                nameof(TargetCollection),
                typeof(PlusPropertyDescriptorsCollection),
                typeof(PropertyTransfer),
                new PropertyMetadata(default, OnTargetCollectionChanged));

        public PropertyTransfer()
        {
            this.CommandMoveUp = new DelegateCommand(
                (param) =>
                {
                    var index = this.TargetCollection.IndexOf(this.TargetSelectedItem);
                    int newIndex = index - 1;
                    if (newIndex < 0)
                        return;
                    this.TargetCollection.Move(index, newIndex);
                    this.TargetSelectedItem = this.TargetCollection[newIndex];
                },
                o => this.TargetSelectedItem != null);

            this.CommandMoveDown = new DelegateCommand(
                (param) =>
                {
                    var index = this.TargetCollection.IndexOf(this.TargetSelectedItem);
                    int newIndex = index + 1;
                    if (newIndex >= this.TargetCollection.Count)
                        return;
                    this.TargetCollection.Move(index, newIndex);
                    this.TargetSelectedItem = this.TargetCollection[newIndex];
                },
                o => this.TargetSelectedItem != null);

            this.CommandClear = new DelegateCommand(
                (param) =>
                {
                    TMPApplication.TMPApp.ShowQuestion("Очистить список используемых полей?",
                        onYes: () =>
                        {
                            this.TargetCollection.Clear();
                            this.SourceCollection?.Refresh();
                            this.RaisePropertyChanged(nameof(this.AllSourceCollectionItemsUsed));
                            this.RaisePropertyChanged(nameof(this.HasTargetItems));

                            var binding = this.GetBindingExpression(PropertyTransfer.TargetCollectionProperty);
                            binding.UpdateSource();
                        },
                        onNo: () => { });
                },
                o => this.TargetCollection != null && this.TargetCollection.Count > 0);

            this.CommandRemoveFromTarget = new DelegateCommand(
                (param) =>
                {
                    this.TargetCollection.Remove(this.TargetSelectedItem);

                    this.SourceCollection?.Refresh();
                    this.RaisePropertyChanged(nameof(this.AllSourceCollectionItemsUsed));
                    this.RaisePropertyChanged(nameof(this.HasTargetItems));

                    var binding = this.GetBindingExpression(PropertyTransfer.TargetCollectionProperty);
                    binding.UpdateSource();
                },
                o => this.TargetSelectedItem != null);

            this.CommandAddToTarget = new DelegateCommand(
                (param) =>
                {
                    this.TargetCollection.Add(this.SourceSelectedItem);
                    this.SourceCollection?.Refresh();
                    this.RaisePropertyChanged(nameof(this.AllSourceCollectionItemsUsed));
                    this.RaisePropertyChanged(nameof(this.HasTargetItems));
                },
                o => this.SourceSelectedItem != null && this.TargetCollection != null && this.TargetCollection.Contains(this.SourceSelectedItem) == false);

            this.InitializeComponent();
        }

        public bool HasSourceItems => this.SourceCollection.IsEmpty == false;

        public ICollectionView SourceCollection
        {
            get => (ICollectionView)this.GetValue(SourceCollectionProperty);
            set
            {
                if (this.existingFilter != null && this.SourceCollection != null)
                {
                    this.SourceCollection.Filter = this.existingFilter;
                }

                this.SetValue(SourceCollectionProperty, value);
                this.sourceCollectionItemsCount = value.SourceCollection.Cast<object>().Count();
                this.SetFilter(value);
            }
        }

        public bool HasTargetItems => (this.TargetCollection is ICollection collection1) && collection1.Count > 0;

        public PlusPropertyDescriptorsCollection TargetCollection
        {
            get => (PlusPropertyDescriptorsCollection)this.GetValue(TargetCollectionProperty);
            set => this.SetValue(TargetCollectionProperty, value);
        }

        public bool AllSourceCollectionItemsUsed => this.SourceCollection.IsEmpty && this.sourceCollectionItemsCount > 0;

        public PlusPropertyDescriptor SourceSelectedItem { get; set; }

        public PlusPropertyDescriptor TargetSelectedItem { get; set; }

        public ICommand CommandMoveUp { get; }

        public ICommand CommandMoveDown { get; }

        public ICommand CommandDelete { get; }

        public ICommand CommandClear { get; }

        public ICommand CommandRemoveFromTarget { get; }

        public ICommand CommandAddToTarget { get; }

        private static void OnSourceCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyTransfer propertyTransfer = (PropertyTransfer)d;

            propertyTransfer.sourceCollectionItemsCount = (e.NewValue is ICollection collection) ? collection.Count : ((IEnumerable)e.NewValue).Cast<object>().Count();

            propertyTransfer.SetFilter(e.NewValue as ICollectionView);

            propertyTransfer.RaisePropertyChanged(nameof(propertyTransfer.HasSourceItems));
            propertyTransfer.RaisePropertyChanged(nameof(propertyTransfer.AllSourceCollectionItemsUsed));
        }

        private static void OnTargetCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyTransfer propertyTransfer = (PropertyTransfer)d;

            (propertyTransfer.SourceCollection as ICollectionView)?.Refresh();

            propertyTransfer.RaisePropertyChanged(nameof(HasTargetItems));
            propertyTransfer.RaisePropertyChanged(nameof(propertyTransfer.AllSourceCollectionItemsUsed));
        }

        private void SetFilter(ICollectionView collectionView)
        {
            if (collectionView != null && collectionView.CanFilter)
            {
                if (collectionView.Filter != null)
                {
                    this.existingFilter = collectionView.Filter;
                }

                collectionView.Filter = this.Filter;
            }
        }

        private bool Filter(object o)
        {
            PlusPropertyDescriptor plusPropertyDescriptor = (PlusPropertyDescriptor)o;
            if (plusPropertyDescriptor != null && this.TargetCollection != null)
            {
                bool b1 = true;// this.existingFilter == null || this.existingFilter.Invoke(o);
                bool b2 = this.TargetCollection.Contains(plusPropertyDescriptor) == false;
                return b1 && b2;
            }
            else
            {
                return false;
            }
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (propertyName != null && TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                {
                    throw new System.Exception(msg);
                }
                else
                {
                    System.Diagnostics.Debug.Fail(msg);
                }
            }
        }

        [System.Runtime.Serialization.IgnoreDataMember]
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(storage, value))
            {
                storage = value;
                this.RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.VerifyPropertyName(propertyName);

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}
