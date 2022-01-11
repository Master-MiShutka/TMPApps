namespace DataGridWpf
{
    using System;
    using System.Windows.Data;
    using SysDataGridColumn = System.Windows.Controls.DataGridColumn;

    public static class Factory
    {
        public static IDataGridWpfColumn CreateWpfColumn(SysDataGridColumn dataGridColumn, System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            IDataGridWpfColumn column = CreateWpfColumnByDataType(e.PropertyType);

            column.FieldName = e.PropertyName;
            column.IsColumnFiltered = true;

            TransferProperties(dataGridColumn, column);

            if (column is System.Windows.Controls.DataGridBoundColumn dgbc)
            {
                dgbc.Binding = new Binding(e.PropertyName) { Mode = BindingMode.OneWay };
            }

            return column;
        }

        public static IDataGridWpfColumn ToDataGridWpfColumn(SysDataGridColumn dataGridColumn)
        {
            IDataGridWpfColumn column = dataGridColumn switch
            {
                System.Windows.Controls.DataGridTextColumn => new WpfDataGridTextColumn(),
                System.Windows.Controls.DataGridComboBoxColumn => new WpfDataGridComboBoxColumn(),
                System.Windows.Controls.DataGridCheckBoxColumn => new WpfDataGridCheckBoxColumn(),
                System.Windows.Controls.DataGridTemplateColumn => new WpfDataGridTemplateColumn(),
                _ => new WpfDataGridTextColumn(),
            };

            TransferProperties(dataGridColumn, column);

            if (dataGridColumn is System.Windows.Controls.DataGridCheckBoxColumn)
            {
                return column;
            }

            CloneBinding(dataGridColumn as System.Windows.Controls.DataGridBoundColumn, column as System.Windows.Controls.DataGridBoundColumn);

            // if (dataGridColumn is System.Windows.Controls.DataGridTemplateColumn dgtc)
            // {
            //    var c3 = column as System.Windows.Controls.DataGridTemplateColumn;
            // }

            // if (dataGridColumn is System.Windows.Controls.DataGridComboBoxColumn dgcbc)
            // {
            //    var c2 = column as System.Windows.Controls.DataGridComboBoxColumn;
            // }
            return column;
        }

        public static IDataGridWpfColumn ToDataGridWpfColumn(DataGridWpfColumnViewModel dataGridWpfColumn)
        {
            SysDataGridColumn dataGridColumn = dataGridWpfColumn.DataType switch
            {
                Type t when t == typeof(bool) => new WpfDataGridCheckBoxColumn(),
                Type t when t == typeof(string) => new WpfDataGridTextColumn(),
                Type t when t == typeof(byte) ||
                             t == typeof(sbyte) ||
                             t == typeof(int) ||
                             t == typeof(uint) ||
                             t == typeof(short) ||
                             t == typeof(ushort) ||
                             t == typeof(long) ||
                             t == typeof(ulong) ||
                             t == typeof(float) ||
                             t == typeof(double) ||
                             t == typeof(decimal) ||
                             t == typeof(bool) ||
                             t == typeof(char) || t.BaseType == typeof(Enum) => new WpfDataGridTextColumn(),
                Type t when t == typeof(object) => new WpfDataGridTextColumn() { SortMemberPath = dataGridWpfColumn.FieldName }, // DataGridWpf.DataGridTemplateColumn(),
                _ => new WpfDataGridTextColumn(),
            };

            dataGridColumn.SetValue(FilterDataGrid.ColumnViewModelProperty, dataGridWpfColumn);

            (dataGridColumn as IDataGridWpfColumn).FieldName = dataGridWpfColumn.FieldName;

            dataGridColumn.Header = dataGridWpfColumn.Title;
            dataGridColumn.Visibility = dataGridWpfColumn.IsVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (dataGridWpfColumn.DisplayIndex != -1)
            {
                dataGridColumn.DisplayIndex = dataGridWpfColumn.DisplayIndex;
            }

            if (dataGridColumn is System.Windows.Controls.DataGridBoundColumn dgbc)
            {
                dgbc.Binding = new Binding()
                {
                    Mode = BindingMode.OneWay,
                    Path = new System.Windows.PropertyPath(dataGridWpfColumn.FieldName),
                    StringFormat = dataGridWpfColumn.CellContentStringFormat,
                };
            }

            if (string.IsNullOrEmpty(dataGridWpfColumn.CellContentExportStringFormat) == false)
            {
                dataGridColumn.ClipboardContentBinding = new Binding()
                {
                    Mode = BindingMode.OneWay,
                    Path = new System.Windows.PropertyPath(dataGridWpfColumn.FieldName),
                    StringFormat = dataGridWpfColumn.CellContentExportStringFormat,
                };
            }

            return dataGridColumn as IDataGridWpfColumn;
        }

        public static DataGridWpfColumnViewModel GetDataGridWpfColumnViewModel(SysDataGridColumn dataGridColumn)
        {
            DataGridWpfColumnViewModel dataGridWpfColumn = new()
            {
                Title = (string)dataGridColumn.Header,
                IsVisible = dataGridColumn.Visibility == System.Windows.Visibility.Visible,
                DisplayIndex = dataGridColumn.DisplayIndex,
            };

            if (dataGridColumn is System.Windows.Controls.DataGridBoundColumn boundColumn && boundColumn.Binding != null)
            {
                dataGridWpfColumn.FieldName = ((Binding)boundColumn.Binding).Path.Path;
                dataGridWpfColumn.CellContentStringFormat = ((Binding)boundColumn.Binding).StringFormat;
            }

            dataGridWpfColumn.Tag = dataGridColumn;

            return dataGridWpfColumn;
        }

        #region Private

        private static void TransferProperties(SysDataGridColumn dataGridColumn, IDataGridWpfColumn column)
        {
            var c = column as SysDataGridColumn;
            if (c == null)
            {
                System.Diagnostics.Debugger.Break();
            }

            c.Header = dataGridColumn.Header;
            c.Visibility = dataGridColumn.Visibility;
            if (dataGridColumn.DisplayIndex != -1)
            {
                c.DisplayIndex = dataGridColumn.DisplayIndex;
            }
        }

        private static IDataGridWpfColumn CreateWpfColumnByDataType(Type type)
        {
            return type switch
            {
                Type t when t.IsEnum => new WpfDataGridTextColumn(),
                Type t when t == typeof(string) => new WpfDataGridTextColumn(),
                Type t when t == typeof(bool) => new WpfDataGridCheckBoxColumn(),
                Type t when t == typeof(Uri) => new WpfDataGridTextColumn(),
                _ => new WpfDataGridTextColumn(),
            };
        }

        private static BindingBase CloneBinding(BindingBase bindingBase, object source)
        {
            var binding = bindingBase as Binding;
            if (binding != null)
            {
                var result = new Binding
                {
                    Source = source,
                    AsyncState = binding.AsyncState,
                    BindingGroupName = binding.BindingGroupName,
                    BindsDirectlyToSource = binding.BindsDirectlyToSource,
                    Converter = binding.Converter,
                    ConverterCulture = binding.ConverterCulture,
                    ConverterParameter = binding.ConverterParameter,

                    // ElementName = binding.ElementName,
                    FallbackValue = binding.FallbackValue,
                    IsAsync = binding.IsAsync,
                    Mode = binding.Mode,
                    NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                    NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                    NotifyOnValidationError = binding.NotifyOnValidationError,
                    Path = binding.Path,

                    // RelativeSource = binding.RelativeSource,
                    StringFormat = binding.StringFormat,
                    TargetNullValue = binding.TargetNullValue,
                    UpdateSourceExceptionFilter = binding.UpdateSourceExceptionFilter,
                    UpdateSourceTrigger = binding.UpdateSourceTrigger,
                    ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
                    ValidatesOnExceptions = binding.ValidatesOnExceptions,
                    XPath = binding.XPath,
                };

                foreach (var validationRule in binding.ValidationRules)
                {
                    result.ValidationRules.Add(validationRule);
                }

                return result;
            }

            var multiBinding = bindingBase as MultiBinding;
            if (multiBinding != null)
            {
                var result = new MultiBinding
                {
                    BindingGroupName = multiBinding.BindingGroupName,
                    Converter = multiBinding.Converter,
                    ConverterCulture = multiBinding.ConverterCulture,
                    ConverterParameter = multiBinding.ConverterParameter,
                    FallbackValue = multiBinding.FallbackValue,
                    Mode = multiBinding.Mode,
                    NotifyOnSourceUpdated = multiBinding.NotifyOnSourceUpdated,
                    NotifyOnTargetUpdated = multiBinding.NotifyOnTargetUpdated,
                    NotifyOnValidationError = multiBinding.NotifyOnValidationError,
                    StringFormat = multiBinding.StringFormat,
                    TargetNullValue = multiBinding.TargetNullValue,
                    UpdateSourceExceptionFilter = multiBinding.UpdateSourceExceptionFilter,
                    UpdateSourceTrigger = multiBinding.UpdateSourceTrigger,
                    ValidatesOnDataErrors = multiBinding.ValidatesOnDataErrors,
                    ValidatesOnExceptions = multiBinding.ValidatesOnDataErrors,
                };

                foreach (var validationRule in multiBinding.ValidationRules)
                {
                    result.ValidationRules.Add(validationRule);
                }

                foreach (var childBinding in multiBinding.Bindings)
                {
                    result.Bindings.Add(CloneBinding(childBinding, source));
                }

                return result;
            }

            var priorityBinding = bindingBase as PriorityBinding;
            if (priorityBinding != null)
            {
                var result = new PriorityBinding
                {
                    BindingGroupName = priorityBinding.BindingGroupName,
                    FallbackValue = priorityBinding.FallbackValue,
                    StringFormat = priorityBinding.StringFormat,
                    TargetNullValue = priorityBinding.TargetNullValue,
                };

                foreach (var childBinding in priorityBinding.Bindings)
                {
                    result.Bindings.Add(CloneBinding(childBinding, source));
                }

                return result;
            }

            throw new NotSupportedException("Failed to clone binding");
        }

        private static void CloneBinding(System.Windows.Controls.DataGridBoundColumn sourceColumn, System.Windows.Controls.DataGridBoundColumn targetColumn)
        {
            if (sourceColumn != null && (sourceColumn.Binding as Binding) != null)
            {
                var binding = sourceColumn.Binding as Binding;

                targetColumn.Binding = CloneBinding(binding, null);

                if (sourceColumn.ClipboardContentBinding != null && sourceColumn.ClipboardContentBinding is Binding binding1)
                {
                    targetColumn.ClipboardContentBinding = CloneBinding(binding1, null);
                }
            }
        }

        #endregion
    }
}
