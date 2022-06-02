namespace ItemsFilter.Initializer
{
    using ItemsFilter.Model;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Filter initializer for BooleanFilter.
    /// </summary>
    public class BooleanFilterInitializer : PropertyFilterInitializer
    {
        /// <summary>
        /// Generate new instance of BooleanFilter class, if it is possible for a pair of filterPresenter and propertyInfo.
        /// </summary>
        /// <param name="filterPresenter">FilterPresenter, which can be attached Filter</param>
        /// <param name="key">Key, used as the name for binding property in filterPresenter.Parent collection.</param>
        /// <returns>Instance of EnumFilter class or null.</returns>
        protected override PropertyFilter NewFilter(FilterPresenter filterPresenter, ItemPropertyInfo propertyInfo)
        {
            Debug.Assert(filterPresenter != null);
            Debug.Assert(propertyInfo != null);
            Type propertyType = propertyInfo.PropertyType;
            if (filterPresenter.ItemProperties.Contains(propertyInfo)
                && typeof(IComparable).IsAssignableFrom(propertyType)
                && propertyType == typeof(bool))
            {
                return new BooleanFilter(propertyInfo);
            }

            return null;
        }
    }
}
