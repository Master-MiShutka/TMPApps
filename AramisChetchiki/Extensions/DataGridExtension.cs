using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.DataGrid;

namespace TMP.WORK.AramisChetchiki.Extensions
{
    public static class DataGridExtensions
    {
        public static IEnumerable<ColumnBase> BuildColumns(IEnumerable<Properties.TableField> fields)
        {
            int index = 0;
            foreach (var item in fields)
                item.DisplayOrder = index++;
            List<ColumnBase> result = new List<ColumnBase>();
            foreach (Properties.TableField field in fields)
            {
                if (field == null)
                    System.Diagnostics.Debugger.Break();
                TypeCode typecode = Type.GetTypeCode(field.Type);

                if (Nullable.GetUnderlyingType(field.Type) != null)
                {
                    typecode = Type.GetTypeCode(Nullable.GetUnderlyingType(field.Type));
                }

                switch (typecode)
                {
                    case TypeCode.Boolean:

                        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(DataGridCheckBox));
                        factory.SetBinding(DataGridCheckBox.IsCheckedProperty, new Binding(field.Name));

                        result.Add(new UnboundColumn()
                        {
                            Title = field.DisplayName,
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible,
                            CellContentTemplate = new DataTemplate() { VisualTree = factory }
                        });
                        break;
                    case TypeCode.Double:
                        result.Add(new Column()
                        {
                            Title = field.DisplayName,
                            CellContentStringFormat="N0",
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible
                        });
                        break;
                    case TypeCode.DateTime:
                        result.Add(new Column()
                        {
                            Title = field.DisplayName,
                            CellContentStringFormat = "dd.MM.yyyy",
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible
                        });
                        break;
                    default:
                        result.Add(new Column()
                        {
                            Title = field.DisplayName,
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible
                        });
                        break;
                }
            }
            return result;
        }
    }
}
