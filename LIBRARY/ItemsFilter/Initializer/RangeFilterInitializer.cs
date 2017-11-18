using System;
using System.ComponentModel;
using ItemsFilter.Model;
using System.Diagnostics;

namespace ItemsFilter.Initializer {
    /// <summary>
    /// Define RangeFilter initializer.
    /// </summary>
    public class RangeFilterInitializer : PropertyFilterInitializer {
        #region IPropertyFilterInitializer Members

        protected override PropertyFilter NewFilter(FilterPresenter filterPresenter, ItemPropertyInfo propertyInfo) {
            Debug.Assert(filterPresenter != null);
            Debug.Assert(propertyInfo != null);

            Type propertyType = propertyInfo.PropertyType;
            if (filterPresenter.ItemProperties.Contains(propertyInfo)
                && typeof(IComparable).IsAssignableFrom(propertyType)
                && propertyType != typeof(String)
                && propertyType != typeof(bool)
                && !propertyType.IsEnum
                )
            {
                return (PropertyFilter)Activator.CreateInstance(typeof(RangeFilter<>).MakeGenericType(propertyInfo.PropertyType), propertyInfo);
            }
            return null;
        }

        #endregion
    }
}
