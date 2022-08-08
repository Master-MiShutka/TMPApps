namespace ItemsFilter.Model
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using ItemsFilter.View;

    /// <summary>
    /// Define boolean filter.
    /// </summary>
    [View(typeof(BooleanFilterView))]
    public class BooleanFilter : PropertyFilter, IBooleanFilter
    {
        private readonly Func<object, bool?> getter;
        private bool? value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFilter"/> class.
        /// </summary>
        /// <param name="getter">Func that return from item bool? value to compare.</param>
        protected BooleanFilter(Func<object, bool?> getter)
        {
            Debug.Assert(getter != null, "getter is null.");
            this.getter = getter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        public BooleanFilter(ItemPropertyInfo propertyInfo)
            : base()
        {
            // if (!typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType))
            //    throw new ArgumentOutOfRangeException("propertyInfo", "typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            Debug.Assert(propertyInfo != null, "propertyInfo is null.");
            Debug.Assert(typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType), "The typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            this.PropertyInfo = propertyInfo;

            Func<object, object> getterItem = (o) =>
            {
                var d = this.PropertyInfo.Descriptor;
                var pd = (PropertyDescriptor)d;
                var result = pd.GetValue(o);
                return result;
            };
            this.getter = t =>
            {
                var result = getterItem(t);
                return (bool)result;
            };

            this.Name = ItemsFilter.Resources.Strings.BoolText;
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
                    bool? toCompare = this.getter(e.Item);

                    e.Accepted = this.value.HasValue == false || this.value.Value.CompareTo(toCompare) == 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value to look for.
        /// </summary>
        /// <value>The value.</value>
        public bool? Value
        {
            get => this.value;

            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.IsActive = value.HasValue;
                    this.OnIsActiveChanged();
                    this.RaiseFilterChanged();
                    this.RaisePropertyChanged(nameof(this.Value));
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
                this.Value = null;
            }

            base.OnIsActiveChanged();
        }
    }
}
