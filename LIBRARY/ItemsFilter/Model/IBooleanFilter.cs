namespace ItemsFilter.Model
{
    using System;

    /// <summary>
    /// Defines the contract for a boolean filter.
    /// </summary>
    public interface IBooleanFilter : IPropertyFilter
    {
        /// <summary>
        /// Gets or sets the value to look for.
        /// </summary>
        bool? Value { get; set; }
    }
}
