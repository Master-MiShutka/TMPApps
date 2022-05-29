namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Input;
    using DataGridWpf;
    using TMP.Shared;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;

    public static class Utils
    {
        public static double? Median<TColl, TValue>(this IEnumerable<TColl> source, Func<TColl, TValue> selector)
        {
            return source.Select<TColl, TValue>(selector).Median();
        }

        public static double? Median<T>(this IEnumerable<T> source)
        {
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
                source = source.Where(x => x != null);

            int count = source.Count();
            if (count == 0)
                return null;

            source = source.OrderBy(n => n);

            int midpoint = count / 2;
            if (count % 2 == 0)
                return (Convert.ToDouble(source.ElementAt(midpoint - 1)) + Convert.ToDouble(source.ElementAt(midpoint))) / 2.0;
            else
                return Convert.ToDouble(source.ElementAt(midpoint));
        }

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

        private static string None = "(нет)";
        private static IList<HierarchicalItem> sortFields = new List<HierarchicalItem>
            {
                new HierarchicalItem() { Name = None, Command = CommandDoSort },
            };

        private static IList<HierarchicalItem> groupFields = new List<HierarchicalItem>
            {
                new HierarchicalItem() { Name = None, Command = CommandDoSort },
            };

        public static IList<HierarchicalItem> SortFields
        {
            get
            {
                if (sortFields.Count == 1)
                {
                    IEnumerable<HierarchicalItem> l1 = ModelHelper.MeterPropertiesNames.Select(a => new HierarchicalItem
                    {
                        Name = a,
                        Command = CommandDoSort,
                        Items = a == None ? null : ModelHelper.MeterPropertiesNames
                            .Where(b => b != None && b != a)
                            .Select(c => new HierarchicalItem(c, CommandDoSort, true)),
                    });
                    foreach (HierarchicalItem item in l1)
                    {
                        sortFields.Add(item);
                    }
                }

                return sortFields;
            }
        }

        public static IList<HierarchicalItem> GroupFields
        {
            get
            {
                if (groupFields.Count == 1)
                {
                    IEnumerable<HierarchicalItem> l2 = ModelHelper.MeterPropertiesNames.Select(a => new HierarchicalItem
                    {
                        Name = a,
                        Command = CommandDoGroup,
                        Items = a == None ? null : ModelHelper.MeterPropertiesNames
                    .Where(b => b != None && b != a)
                    .Select(c => new HierarchicalItem(c, CommandDoGroup, true)
                    {
                        Items = c == None ? null : ModelHelper.MeterPropertiesNames
                            .Where(d => d != None && d != c)
                            .Select(e => new HierarchicalItem(e, CommandDoGroup, true)),
                    }),
                    });
                    foreach (HierarchicalItem item in l2)
                    {
                        groupFields.Add(item);
                    }
                }

                return groupFields;
            }
        }

        public static ICommand CommandDoSort => new DelegateCommand<HierarchicalItem>(DoSort);

        public static ICommand CommandDoGroup => new DelegateCommand<HierarchicalItem>(DoGroup);

        /// <summary>
        /// Перечень полей для сортировки
        /// </summary>
        public static string SortingFields { get; internal set; } = string.Empty;

        /// <summary>
        /// Перечень полей для группировки
        /// </summary>
        public static string GroupingFields { get; internal set; } = string.Empty;

        private static void DoSort(HierarchicalItem field)
        {
            if (field == null)
            {
                return;
            }

            ViewModel.IMainViewModel mainViewModel = TMPApplication.TMPApp.Instance.MainViewModel as ViewModel.IMainViewModel;
            if (mainViewModel == null)
            {
                return;
            }
            ViewModel.IViewModelWithDataView viewModel = mainViewModel.CurrentViewModel as ViewModel.IViewModelWithDataView;
            if (viewModel == null)
            {
                return;
            }

            if (viewModel.View == null)
            {
                return;
            }

            if (viewModel.View.CanSort == false)
            {
                return;
            }

            using (viewModel.View.DeferRefresh())
            {
                SortingFields = string.Empty;
                viewModel.View.SortDescriptions.Clear();
                if (field.Name == None)
                {
                    return;
                }

                Stack<string> stack = new();
                HierarchicalItem item = field;
                while (item != null)
                {
                    stack.Push(item.Name);
                    item = item.Parent;
                }

                string[] values = stack.ToArray();
                SortingFields = string.Join(" > ", values.Select(s => s.Replace("_", " ", AppSettings.StringComparisonMethod))).ToString();
                foreach (string value in values)
                {
                    viewModel.View.SortDescriptions.Add(new SortDescription(value, ListSortDirection.Ascending));
                }
            }
        }

        private static void DoGroup(HierarchicalItem field)
        {
            if (field == null)
            {
                return;
            }

            ViewModel.IMainViewModel mainViewModel = TMPApplication.TMPApp.Instance.MainViewModel as ViewModel.IMainViewModel;
            if (mainViewModel == null)
            {
                return;
            }
            ViewModel.IViewModelWithDataView viewModel = mainViewModel.CurrentViewModel as ViewModel.IViewModelWithDataView;
            if (viewModel == null)
            {
                return;
            }

            if (viewModel.View == null)
            {
                return;
            }

            if (viewModel.View.CanSort == false)
            {
                return;
            }

            using (viewModel.View.DeferRefresh())
            {
                GroupingFields = string.Empty;
                viewModel.View.GroupDescriptions.Clear();
                if (field.Name == None)
                {
                    return;
                }

                Stack<string> stack = new();
                HierarchicalItem item = field;
                while (item != null)
                {
                    stack.Push(item.Name);
                    item = item.Parent;
                }

                string[] values = stack.ToArray();
                GroupingFields = string.Join(" > ", values.Select(s => s.Replace("_", " ", AppSettings.StringComparisonMethod)));
                foreach (string value in values)
                {
                    viewModel.View.GroupDescriptions.Add(new PropertyGroupDescription(value));
                }
            }
        }
    }
}
