namespace ItemsFilter.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Linq;
    using ItemsFilter.View;

    /// <summary>
    /// Base class for filter that using list of values.
    /// </summary>
    [View(typeof(MultiValueFilterView))]
    public abstract class EqualFilter : PropertyFilter, IMultiValueFilter, IFilter
    {
        private ObservableCollection<object> selectedValues;
        private ReadOnlyObservableCollection<object> readonlySelectedValues;

        protected IEnumerable availableValues;
        private int availableValuesCount;

        /// <summary>
        /// Initialize new instance of EqualFilter from deriver class.
        /// </summary>
        protected EqualFilter()
        {
            base.Name = ItemsFilter.Resources.Strings.EqualText;
        }

        public ReadOnlyObservableCollection<object> SelectedValues => this.readonlySelectedValues;

        public virtual IEnumerable AvailableValues
        {
            get => this.availableValues;

            set
            {
                if (this.availableValues != value)
                {
                    this.availableValues = value;
                    this.availableValuesCount = this.availableValues.Cast<object>().Count();
                    this.RaisePropertyChanged(nameof(this.AvailableValues));

                    this.selectedValues = new ObservableCollection<object>(this.AvailableValues.Cast<object>());
                    this.readonlySelectedValues = new ReadOnlyObservableCollection<object>(this.selectedValues);

                    this.RaisePropertyChanged(nameof(this.SelectedValues));
                }
            }
        }

        protected override void OnIsActiveChanged()
        {
            if (!this.IsActive)
            {
                this.selectedValues = new ObservableCollection<object>(this.AvailableValues.Cast<object>());
                // this.selectedValues.Clear();
            }

            base.OnIsActiveChanged();
        }

        public void SelectedValuesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (this.selectedValues.Contains(item) == false)
                        this.selectedValues.Add(item);
                }

                this.IsActive = true;
            }

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    if (this.selectedValues.Contains(item) == true)
                        this.selectedValues.Remove(item);
                }

                this.IsActive = this.selectedValues.Count != this.availableValuesCount;
            }

            this.OnIsActiveChanged();
            this.RaiseFilterChanged();
        }
    }

    /// <summary>
    /// Defines the logic for equality filter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EqualFilter<T> : EqualFilter, IMultiValueFilter
    {
        protected readonly Func<object, object> getter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="getter">Func that return from item values to compare.</param>
        protected EqualFilter(Func<object, object> getter)
        {
            Debug.Assert(getter != null, "getter is null.");
            this.getter = getter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="getter">Func that return values to compare from item.</param>
        /// <param name="availableValues">Predefined set of available values.</param>
        protected internal EqualFilter(Func<object, object> getter, IEnumerable availableValues)
            : this(getter)
        {
            this.availableValues = availableValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        public EqualFilter(ItemPropertyInfo propertyInfo)
            : base()
        {
            Debug.Assert(propertyInfo != null, "propertyInfo is null.");
            Debug.Assert(propertyInfo.PropertyType == typeof(T), "Invalid propertyInfo.PropertyType, the return type is not matching the class generic type.");
            base.PropertyInfo = propertyInfo;
            this.getter = ((PropertyDescriptor)this.PropertyInfo.Descriptor).GetValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualFilter&lt;T&gt;"/> class
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="availableValues">Predefined set of available values.</param>
        public EqualFilter(ItemPropertyInfo propertyInfo, IEnumerable availableValues)
            : this(propertyInfo)
        {
            this.availableValues = availableValues;
        }

        /// <summary>
        /// Set of available values that can be include in filter.
        /// </summary>
        public override IEnumerable AvailableValues
        {
            get => this.availableValues;

            set
            {
                if (this.availableValues != value)
                {
                    this.availableValues = value;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified target is a match.
        /// </summary>
        public override void IsMatch(FilterPresenter sender, FilterEventArgs e)
        {
            if (e.Accepted)
            {
                if (e.Item == null)
                {
                    e.Accepted = false;
                }
                else
                {
                    object value = this.getter(e.Item);
                    e.Accepted = this.SelectedValues.Contains(value) == false;
                }
            }
        }
    }
}

