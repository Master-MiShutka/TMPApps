namespace TMP.Shared
{
    using System;
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
            System.Diagnostics.Debug.Assert(viewType != null & typeof(FrameworkElement).IsAssignableFrom(viewType));
            this.viewType = viewType;
        }
    }
}
