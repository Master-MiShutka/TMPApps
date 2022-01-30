namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataGridWpf;
    using TMP.Shared;
    using TMP.WORK.AramisChetchiki.Model;

    public static class Utils
    {
        public static Tuple<IEnumerable<DataGridWpfColumnViewModel>, List<string>> GenerateColumns(IEnumerable<PlusPropertyDescriptor> fields)
        {
            IEnumerable<DataGridWpfColumnViewModel> columns = BuildColumns(fields);
            return GenerateColumns(columns);
        }

        public static Tuple<IEnumerable<DataGridWpfColumnViewModel>, List<string>> GenerateColumns(IEnumerable<DataGridWpfColumnViewModel> columns)
        {
            List<string> tableColumnsFields = new();
            if (columns != null)
            {
                foreach (DataGridWpfColumnViewModel column in columns.Cast<DataGridWpfColumnViewModel>())
                {
                    tableColumnsFields.Add(column.FieldName);
                }
            }

            return new Tuple<IEnumerable<DataGridWpfColumnViewModel>, List<string>>(columns, tableColumnsFields);
        }

        public static Tuple<IEnumerable<DataGridWpfColumnViewModel>, IEnumerable<string>> GenerateMeterColumns(TableViewKinds tableViewKind)
        {
            IEnumerable<PlusPropertyDescriptor> propertyDescriptors = ModelHelper.GetPropertyDescriptors(typeof(Model.Meter), tableViewKind);
            IEnumerable<DataGridWpfColumnViewModel> collection = BuildColumns(propertyDescriptors);

            IEnumerable<string> fieldNames = propertyDescriptors?.Select(m => m.Name);

            return new Tuple<IEnumerable<DataGridWpfColumnViewModel>, IEnumerable<string>>(collection, fieldNames);
        }

        public static IList<Model.Field> ToFields(this IEnumerable<DataGridWpfColumnViewModel> dataGridBoundColumns)
        {
            IList<Model.Field> result = new List<Model.Field>();

            foreach (DataGridWpfColumnViewModel column in dataGridBoundColumns.Cast<DataGridWpfColumnViewModel>())
            {
                result.Add(new Field()
                {
                    Header = column.Title.ToString(),
                    FieldName = column.FieldName,
                    IsChecked = column.IsVisible,
                    IsVisible = column.IsVisible,
                    DisplayIndex = column.DisplayIndex,
                });
            }

            return result;
        }

        public static IEnumerable<Model.Field> ToFields(this System.ComponentModel.SortDescriptionCollection sortDescriptionCollection)
        {
            IList<Model.Field> result = new List<Model.Field>();
            if (sortDescriptionCollection != null)
            {
                foreach (System.ComponentModel.SortDescription item in sortDescriptionCollection)
                {
                    result.Add(new Field(item.PropertyName) { SortDirection = item.Direction });
                }
            }

            return result;
        }

        public static IEnumerable<DataGridWpfColumnViewModel> BuildColumns(IEnumerable<PlusPropertyDescriptor> fields)
        {
            if (fields == null)
            {
                return null;
            }

            int index = 0;
            foreach (PlusPropertyDescriptor item in fields)
            {
                item.Order = index++;
            }

            List<DataGridWpfColumnViewModel> result = new();
            foreach (PlusPropertyDescriptor field in fields)
            {
                if (field == null)
                {
                    System.Diagnostics.Debugger.Break();
                }

                TypeCode typecode = Type.GetTypeCode(field.PropertyType);

                if (Nullable.GetUnderlyingType(field.PropertyType) != null)
                {
                    typecode = Type.GetTypeCode(Nullable.GetUnderlyingType(field.PropertyType));
                }

                DataGridWpfColumnViewModel cvm = new DataGridWpfColumnViewModel()
                {
                    FieldName = field.Name,
                    Title = field.DisplayName,
                    DisplayIndex = field.Order,
                    IsVisible = field.IsVisible,
                    GroupName = field.GroupName,
                    CellContentStringFormat = field.DataFormatString,
                    CellContentExportStringFormat = field.ExportFormatString,
                };

                cvm.DataType = field.PropertyType;
                result.Add(cvm);
            }

            return result;
        }

        public static string ConvertFromTitleCase(string str)
        {
            if (str?.Length == 0)
            {
                return string.Empty;
            }

            string result = str.Substring(0, 1);
            for (int ind = 1; ind < str.Length; ind++)
            {
                char letter = str[ind];
                if (char.IsUpper(letter))
                {
                    result += " ";
                    result += char.ToLower(letter, AppSettings.CurrentCulture);
                }
                else
                {
                    result += letter;
                }
            }

            return result;
        }
    }
}
