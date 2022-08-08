namespace TMP.Shared.Common
{
    using System;

    /// <summary>
    /// Specify the [ModelView] class that present model.
    /// </summary>
    public class ViewAttribute : Attribute
    {
        private readonly Type viewType;

        public Type ViewType => this.viewType;

        public ViewAttribute() { }

        public ViewAttribute(Type viewType)
        {
            this.viewType = viewType;
        }
    }
}
