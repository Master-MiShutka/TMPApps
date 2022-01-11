namespace ItemsFilter.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows;

    /// <summary>
    /// Specify the [ModelView] class that present model.
    /// </summary>
    public class ViewAttribute : Attribute
    {
        private readonly Type viewType;

        public Type ViewType => this.viewType;

        public ViewAttribute(Type viewType = null)
        {
            Debug.Assert(viewType == null || typeof(FrameworkElement).IsAssignableFrom(viewType));
            this.viewType = viewType;
        }
    }
}
