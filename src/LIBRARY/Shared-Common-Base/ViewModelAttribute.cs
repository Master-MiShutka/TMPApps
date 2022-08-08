namespace TMP.Shared.Common
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Indicate that class can be use as View, have property Model and constructor width single parameter Model.
    /// </summary>
    public class ViewModelAttribute : Attribute
    {
        private readonly Type viewModelType;

        public Type ViewModelType => this.viewModelType;

        public ViewModelAttribute() { }

        public ViewModelAttribute(Type viewModelType)
        {
            System.Diagnostics.Debug.Assert(viewModelType != null & viewModelType.GetInterfaces().Contains(typeof(INotifyPropertyChanged)));
            this.viewModelType = viewModelType;
        }
    }
}
