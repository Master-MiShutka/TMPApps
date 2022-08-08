namespace ItemsFilter.Model
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using ItemsFilter.View;

    /// <summary>
    /// Defines the range filter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [View(typeof(RangeFilterView))]
    public class RangeFilter<T> : PropertyFilter, IRangeFilter<T>
        where T : struct, IComparable
    {
        private Func<object, T> getter;
        private T? compareTo = null;
        private T? compareFrom = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="getter">Func that return from item values to compare.</param>
        protected RangeFilter(Func<object, T> getter)
        {
            Debug.Assert(getter != null, "getter is null.");
            this.getter = getter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        public RangeFilter(ItemPropertyInfo propertyInfo)
            : base()
        {
            // if (!typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType))
            //    throw new ArgumentOutOfRangeException("propertyInfo", "typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            Debug.Assert(propertyInfo != null, "propertyInfo is null.");
            Debug.Assert(typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType), "The typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            base.PropertyInfo = propertyInfo;
            Func<object, object> getterItem = ((PropertyDescriptor)this.PropertyInfo.Descriptor).GetValue;
            this.getter = t => (T)getterItem(t);
            base.Name = ItemsFilter.Resources.Strings.InRangeText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="compareFrom">Minimum value.</param>
        /// <param name="CompareTo">Maximum value.</param>
        public RangeFilter(ItemPropertyInfo propertyInfo, T compareFrom, T CompareTo)
            : this(propertyInfo)
        {
            this.compareTo = CompareTo;
            this.compareFrom = compareFrom;
            this.RefreshIsActive();
        }

        /// <summary>
        /// Get or set the minimum value used in the comparison.
        /// If CompareFrom and CompareTo is null, filter deactivated.
        /// </summary>
        public T? CompareFrom
        {
            get => this.compareFrom;
            set
            {
                if (!object.Equals(this.compareFrom, value))
                {
                    this.compareFrom = value;
                    this.RefreshIsActive();
                    this.OnIsActiveChanged();
                    this.RaiseFilterChanged();
                    this.RaisePropertyChanged(nameof(this.CompareFrom));
                }
            }
        }

        /// <summary>
        /// Get or set the maximum value used in the comparison.
        /// If CompareFrom and CompareTo is null, filter deactivated.
        /// </summary>
        public T? CompareTo
        {
            get => this.compareTo;
            set
            {
                if (!object.Equals(this.compareTo, value))
                {
                    this.compareTo = value;
                    this.RefreshIsActive();
                    this.OnIsActiveChanged();
                    this.RaiseFilterChanged();
                    this.RaisePropertyChanged(nameof(this.CompareTo));
                }
            }
        }

        /// <summary>
        /// Provide derived clases IsActiveChanged event.
        /// </summary>
        protected override void OnIsActiveChanged()
        {
            if (!this.IsActive)
            {
                this.CompareFrom = null;
                this.CompareTo = null;
            }

            base.OnIsActiveChanged();
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
                    T value = this.getter(e.Item);
                    e.Accepted = (value.CompareTo(this.compareFrom) >= 0) && (value.CompareTo(this.compareTo) <= 0);
                }
            }
        }

        private void RefreshIsActive()
        {
            base.IsActive = true;
        }

        #region IRangeFilter Members

        object IRangeFilter.CompareFrom
        {
            get => this.CompareFrom;
            set => this.CompareFrom = (T?)value;
        }

        object IRangeFilter.CompareTo
        {
            get => this.CompareTo;
            set => this.CompareTo = (T?)value;
        }

        #endregion
    }
}
