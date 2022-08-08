namespace ItemsFilter.Initializer
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using ItemsFilter.Model;

    /// <summary>
    /// Define LessOrEqualFilter initializer.
    /// </summary>
    public class LessOrEqualFilterInitializer : PropertyFilterInitializer
    {
        /// <summary>
        /// Create LessOrEqualFilter for instance of FilterPresenter, if it is possible.
        /// </summary>
        /// <param name="filterPresenter">FilterPresenter, which can be attached Filter</param>
        /// <param name="key">ItemPropertyInfo for binding to property.</param>
        /// <returns>Instance of LessOrEqualFilter class or null</returns>
        protected override PropertyFilter NewFilter(FilterPresenter filterPresenter, ItemPropertyInfo propertyInfo)
        {
            Debug.Assert(filterPresenter != null);
            Debug.Assert(propertyInfo != null);

            // ItemPropertyInfo propertyInfo = (ItemPropertyInfo)key;
            Type propertyType = propertyInfo.PropertyType;
            if (filterPresenter.ItemProperties.Contains(propertyInfo)
                && typeof(IComparable).IsAssignableFrom(propertyType)
                && propertyType != typeof(string)
                && propertyType != typeof(bool)
                && !propertyType.IsEnum)
            {
                Type gt;
                try
                {
                    gt = typeof(LessOrEqualFilter<>).MakeGenericType(propertyInfo.PropertyType);
                }
                catch (System.ArgumentException ex)
                {
                    return null;
                }

                return (PropertyFilter)Activator.CreateInstance(gt, propertyInfo);
            }

            return null;
        }
    }
}
