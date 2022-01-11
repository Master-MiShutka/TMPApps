namespace ItemsFilter.Model
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using ItemsFilter.View;

    /// <summary>
    /// Define LessOrEqual filter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [View(typeof(ComparableFilterView))]
    public class LessOrEqualFilter<T> : PropertyFilter, IComparableFilter<T>
        where T : struct, IComparable
    {
        protected readonly Func<object, T> getter;
        protected T? compareTo = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="getter">Func that return from item IComparable value to compare.</param>
        protected LessOrEqualFilter(Func<object, T> getter)
        {
            Debug.Assert(getter != null, "getter is null.");
            this.getter = getter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessOrEqualFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        public LessOrEqualFilter(ItemPropertyInfo propertyInfo)
            : base()
        {
            // if (!typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType))
            //    throw new ArgumentOutOfRangeException("propertyInfo", "typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            Debug.Assert(propertyInfo != null, "propertyInfo is null.");
            Debug.Assert(typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType), "The typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            base.PropertyInfo = propertyInfo;

            Func<object, object> getterItem = ((PropertyDescriptor)this.PropertyInfo.Descriptor).GetValue;
            this.getter = t => (T)getterItem(t);
            base.Name = ItemsFilter.Resources.Strings.LessOrEqualText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessOrEqualFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="compareTo">The compare to.</param>
        public LessOrEqualFilter(ItemPropertyInfo propertyInfo, T compareTo)
            : this(propertyInfo)
        {
            this.compareTo = compareTo;
            this.RefreshIsActive();
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
                    e.Accepted = value.CompareTo(this.compareTo) <= 0;
                }
            }
        }

        /// <summary>
        /// Get or set the value used in the comparison. If assign null, filter deactivated.
        /// </summary>
        public T? CompareTo
        {
            get => this.compareTo;

            set
            {
                if (!object.ReferenceEquals(this.compareTo, value))
                {
                    this.compareTo = value;
                    this.RefreshIsActive();
                    this.RaiseFilterStateChanged();
                    this.RaiseFilterChanged();
                    this.RaisePropertyChanged(nameof(this.CompareTo));
                }
            }
        }
        #region IComparableFilter Members

        object IComparableFilter.CompareTo
        {
            get => this.CompareTo;

            set => this.CompareTo = (T?)value;
        }
        #endregion

        /// <summary>
        /// Provide derived clases IsActiveChanged event.
        /// </summary>
        protected override void OnIsActiveChanged()
        {
            if (!this.IsActive)
            {
                this.CompareTo = null;
            }

            base.OnIsActiveChanged();
        }

        private void RefreshIsActive()
        {
            base.IsActive = !object.ReferenceEquals(this.compareTo, null);
        }
    }
}
