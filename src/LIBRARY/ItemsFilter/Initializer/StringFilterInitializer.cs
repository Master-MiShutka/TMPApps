﻿namespace ItemsFilter.Initializer
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using ItemsFilter.Model;

    /// <summary>
    /// Represent initializer for StringFilter.
    /// </summary>
    public class StringFilterInitializer : PropertyFilterInitializer
    {
        /// <summary>
        /// Create new instance of StringFilter, if it is possible for filterPresenter in current state and key.
        /// </summary>
        protected override PropertyFilter NewFilter(FilterPresenter filterPresenter, ItemPropertyInfo propertyInfo)
        {
            Debug.Assert(filterPresenter != null);
            Debug.Assert(propertyInfo != null);
            Type propertyType = propertyInfo.PropertyType;
            if (filterPresenter.ItemProperties.Contains(propertyInfo)
                && propertyType != typeof(bool)
                // && typeof(String).IsAssignableFrom(propertyInfo.PropertyType)
                && !propertyType.IsEnum)
            {
                return new StringFilter(propertyInfo);
            }

            return null;
        }
    }
}
