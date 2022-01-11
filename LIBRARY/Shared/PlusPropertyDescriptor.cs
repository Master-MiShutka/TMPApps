namespace TMP.Shared
{
    using System;
    using System.ComponentModel;

    [Serializable]
    [System.Runtime.Serialization.DataContract]
    [System.Diagnostics.DebuggerDisplay("{DisplayName} :: {Name} :: {GroupName}")]
    public class PlusPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor innerPropertyDescriptor;
        private readonly System.ComponentModel.DataAnnotations.DisplayAttribute displayAttribute;
        private readonly DataFormatAttribute dataFormatAttribute;

        public PlusPropertyDescriptor(PropertyDescriptor inner) : base(inner)
        {
            this.innerPropertyDescriptor = inner;
            this.displayAttribute = this.Attributes[typeof(System.ComponentModel.DataAnnotations.DisplayAttribute)] as System.ComponentModel.DataAnnotations.DisplayAttribute;

            this.dataFormatAttribute = this.Attributes[typeof(DataFormatAttribute)] as DataFormatAttribute;

            if (this.displayAttribute != null)
            {
                this.Order = this.displayAttribute.GetOrder().GetValueOrDefault();
            }
        }

        public Type ModelType => this.innerPropertyDescriptor.ComponentType;

        public override bool IsReadOnly => true;

        public override Type PropertyType => this.innerPropertyDescriptor.PropertyType;

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return this.innerPropertyDescriptor.GetValue(component);
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            this.innerPropertyDescriptor = (PlusPropertyDescriptor)value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public void SetOrder(int order)
        {
            this.Order = order;
        }

        public string DataFormatString => this.dataFormatAttribute?.DataFormatString;

        public string ExportFormatString => this.dataFormatAttribute?.ExportFormatString;

        public string GroupName => this.displayAttribute?.GroupName;

        public int Order { get; set; }

        public bool IsVisible => base.IsBrowsable;

        public override Type ComponentType => this.ModelType;
    }
}
