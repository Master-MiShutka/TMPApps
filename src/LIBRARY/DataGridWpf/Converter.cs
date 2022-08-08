namespace DataGridWpf
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class DataGridWpfColumnConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(DataGridWpfColumnViewModel))
            {
                return true;
            }

            if (sourceType == typeof(System.Windows.Controls.DataGridColumn))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DataGridWpfColumnViewModel))
            {
                return true;
            }

            if (destinationType == typeof(System.Windows.Controls.DataGridColumn))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is System.Windows.Controls.DataGridBoundColumn dataGridColumn)
            {
                return this.ConvertTo(context, culture, dataGridColumn, typeof(DataGridWpfColumnViewModel));
            }

            if (value is DataGridWpfColumnViewModel dataGridWpfColumn)
            {
                return this.ConvertTo(context, culture, dataGridWpfColumn, typeof(System.Windows.Controls.DataGridBoundColumn));
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.Windows.Controls.DataGridBoundColumn) && value is DataGridWpfColumnViewModel dataGridWpfColumn)
            {
                return null; // dataGridWpfColumn.ToDataGridWpfColumn();
            }
            else
                if (destinationType == typeof(DataGridWpfColumnViewModel) && value is System.Windows.Controls.DataGridBoundColumn dataGridColumn)
            {
                return null; // dataGridColumn.GetDataGridWpfColumnViewModel();
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
