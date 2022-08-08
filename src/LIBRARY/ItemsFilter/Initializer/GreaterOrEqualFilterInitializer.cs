namespace ItemsFilter.Initializer
{
    using System;
    using System.ComponentModel;
    using ItemsFilter.Model;

    /// <summary>
    /// Define GreaterOrEqualFilter Initializer.
    /// </summary>
    public class GreaterOrEqualFilterInitializer : LessOrEqualFilterInitializer
    {
        /// <summary>
        /// Create PropertyFilter for instance of FilterPresenter, if it is possible.
        /// </summary>
        /// <param name="filterPresenter">filterPresenter for that filter will be created.</param>
        /// <param name="key">ItemPropertyInfo cpecified property that PropertyFilter will be use.</param>
        /// <returns>Instance of GreaterOrEqualFilter if:
        ///  filterPresenter.ItemProperties.Contains(propertyInfo)
        ///           && typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType)
        ///           && propertyInfo.PropertyType!=typeof(String)
        ///           && propertyInfo.PropertyType != typeof(bool)
        ///           && !propertyType.IsEnum
        ///  otherwise, null.</returns>
        protected override PropertyFilter NewFilter(FilterPresenter filterPresenter, ItemPropertyInfo key)
        {
            if (filterPresenter == null)
            {
                return null;
            }

            if (key == null)
            {
                return null;
            }

            ItemPropertyInfo propertyInfo = (ItemPropertyInfo)key;
            Type propertyType = propertyInfo.PropertyType;
            if (filterPresenter.ItemProperties.Contains(propertyInfo)
                && typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType)
                && propertyInfo.PropertyType != typeof(string)
                && propertyInfo.PropertyType != typeof(bool)
                && !propertyType.IsEnum)
            {
                try
                {
                    return (PropertyFilter)Activator.CreateInstance(typeof(GreaterOrEqualFilter<>).MakeGenericType(propertyInfo.PropertyType), propertyInfo);
                }
                catch (System.ArgumentException ex)
                {
                    return null;
                }
            }

            return null;
        }
    }
}