namespace ItemsFilter.Initializer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Markup;

    /// <summary>
    /// Define a class that represent set of filter initializers.
    /// </summary>
    public class FilterInitializersManager : List<FilterInitializer>, IList<FilterInitializer>, IEnumerable<FilterInitializer>
    {
        private static FilterInitializersManager @default;

        /// <summary>
        /// Represent default instance of FilterInitializersManager that include common used initializers.
        /// </summary>
        public static IEnumerable<FilterInitializer> Default
        {
            get
            {
                if (@default == null)
                {
                    @default = new FilterInitializersManager
                    {
                        new EqualFilterInitializer(),
                        new LessOrEqualFilterInitializer(),
                        new GreaterOrEqualFilterInitializer(),
                        new RangeFilterInitializer(),
                        new StringFilterInitializer(),
                        new EnumFilterInitializer(),
                    };
                }

                return @default;
            }
        }

        /// <summary>
        /// Create empty instance of FilterInitializersManager.
        /// </summary>
        public FilterInitializersManager()
        {
        }

        /// <summary>
        /// Create instance of FilterInitializersManager and add initializers.
        /// </summary>
        /// <param name="initializers">Enumerable of IFilterInitializer to add.</param>
        public FilterInitializersManager(IEnumerable<FilterInitializer> initializers)
            : base()
        {
            foreach (var item in initializers)
            {
                this.Add(item);
            }
        }
    }
}
