namespace ItemsFilter.Model
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using ItemsFilter.View;

    /// <summary>
    /// Defines a string filter
    /// </summary>
    [View(typeof(StringFilterView))]
    public class StringFilter : PropertyFilter, IStringFilter
    {
        private readonly Func<object, string> getter;
        private StringFilterMode filterMode = StringFilterMode.Contains;
        private string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringFilter"/> class.
        /// </summary>
        /// <param name="getter">Func that return from item string value to compare.</param>
        protected StringFilter(Func<object, string> getter)
        {
            Debug.Assert(getter != null, "getter is null.");
            this.getter = getter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringFilter"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="filterMode">The filter mode.</param>
        public StringFilter(ItemPropertyInfo propertyInfo, StringFilterMode filterMode = StringFilterMode.Contains)
            : base()
        {
            // if (!typeof(string).IsAssignableFrom(propertyInfo.PropertyType))
            //    throw new ArgumentOutOfRangeException("propertyInfo", "typeof(string).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            Debug.Assert(propertyInfo != null, "propertyInfo is null.");
            // Debug.Assert(typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType), "The typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) return False.");
            base.PropertyInfo = propertyInfo;
            this.filterMode = filterMode;
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
                return result?.ToString();
            };
            base.Name = Resources.Strings.TextFilterText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringFilter"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <param name="value">The value.</param>
        public StringFilter(ItemPropertyInfo propertyInfo, StringFilterMode filterMode, string value)
            : this(propertyInfo, filterMode)
        {
            this.value = value;
            base.IsActive = !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Gets or sets the string filter mode.
        /// </summary>
        /// <value>The mode.</value>
        public StringFilterMode Mode
        {
            get => this.filterMode;

            set
            {
                if (this.filterMode != value)
                {
                    this.filterMode = value;
                    this.OnIsActiveChanged();
                    this.RaiseFilterChanged();
                    this.RaisePropertyChanged(nameof(this.Mode));
                }
            }
        }

        /// <summary>
        /// Gets or sets the value to look for.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get => this.value;

            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    base.IsActive = !string.IsNullOrEmpty(value);
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
                    string toCompare = this.getter(e.Item);
                    if (string.IsNullOrEmpty(toCompare))
                    {
                        e.Accepted = false;
                    }
                    else
                    {
                        switch (this.filterMode)
                        {
                            case StringFilterMode.Contains:
                                e.Accepted = toCompare.Contains(this.value);
                                break;
                            case StringFilterMode.StartsWith:
                                e.Accepted = toCompare.StartsWith(this.value);
                                break;
                            case StringFilterMode.EndsWith:
                                e.Accepted = toCompare.EndsWith(this.value);
                                break;
                            default:
                                e.Accepted = toCompare.Equals(this.value);
                                break;
                        }
                    }
                }
            }
        }
    }
}
