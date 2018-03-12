using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace TMP.Extensions
{
    public static class LinqExtenions
    {
        /// <summary>
        /// Pivot
        /// <paramref name="source"/>
        /// class Program {
        ///  
        ///     internal class Employee {
        ///         public string Name { get; set; }
        ///         public string Department { get; set; }
        ///         public string Function { get; set; }
        ///         public decimal Salary { get; set; }
        ///     }
        ///  
        ///     static void Main(string[] args) {
        ///  
        ///         var l = new List<Employee>() {
        ///             new Employee() { Name = "Fons", Department = "R&D", Function = "Trainer", Salary = 2000 },
        ///             new Employee() { Name = "Jim", Department = "R&D", Function = "Trainer", Salary = 3000 },
        ///             new Employee() { Name = "Ellen", Department = "Dev", Function = "Developer", Salary = 4000 },
        ///             new Employee() { Name = "Mike", Department = "Dev", Function = "Consultant", Salary = 5000 },
        ///             new Employee() { Name = "Jack", Department = "R&D", Function = "Developer", Salary = 6000 },
        ///             new Employee() { Name = "Demy", Department = "Dev", Function = "Consultant", Salary = 2000 }};
        ///  
        ///         var result1 = l.Pivot(emp => emp.Department, emp2 => emp2.Function, lst => lst.Sum(emp => emp.Salary));
        ///  
        ///         foreach (var row in result1) {
        ///             Console.WriteLine(row.Key);
        ///             foreach (var column in row.Value) {
        ///                 Console.WriteLine("  " + column.Key + "\t" + column.Value);
        ///  
        ///             }
        ///         }
        ///  
        ///         Console.WriteLine("----");
        ///  
        ///         var result2 = l.Pivot(emp => emp.Function, emp2 => emp2.Department, lst => lst.Count());
        ///  
        ///         foreach (var row in result2) {
        ///             Console.WriteLine(row.Key);
        ///             foreach (var column in row.Value) {
        ///                 Console.WriteLine("  " + column.Key + "\t" + column.Value);
        ///  
        ///             }
        ///         }
        ///  
        ///         Console.WriteLine("----");
        ///     }
        /// }
        /// </paramref>
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TFirstKey"></typeparam>
        /// <typeparam name="TSecondKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="firstKeySelector"></param>
        /// <param name="secondKeySelector"></param>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        public static Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>> Pivot<TSource, TFirstKey, TSecondKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TFirstKey> firstKeySelector, Func<TSource, TSecondKey> secondKeySelector, Func<IEnumerable<TSource>, TValue> aggregate)
        {
            var retVal = new Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>>();

            var l = source.ToLookup(firstKeySelector);
            foreach (var item in l)
            {
                var dict = new Dictionary<TSecondKey, TValue>();
                retVal.Add(item.Key, dict);
                var subdict = item.ToLookup(secondKeySelector);
                foreach (var subitem in subdict)
                {
                    dict.Add(subitem.Key, aggregate(subitem));
                }
            }

            return retVal;
        }

        public static DataTable ToPivotTable<T, TColumn, TRow, TData>(
            this IEnumerable<T> source,
            Func<T, TColumn> columnSelector,
            Expression<Func<T, TRow>> rowSelector,
            Func<IEnumerable<T>, TData> dataSelector,
             Func<object, bool> funcRowSelector = null)
        {
            DataTable table = new DataTable();
            var rowName = ((MemberExpression)rowSelector.Body).Member.Name;
            table.Columns.Add(new DataColumn(rowName));
            var columns = source.Select(columnSelector)
                .Distinct()
                .OrderBy(i => i.ToString());

            foreach (var column in columns)
                table.Columns.Add(new DataColumn(column.ToString()));

            var rows = source.GroupBy(rowSelector.Compile())
                             .Select(rowGroup => new
                             {
                                 Key = rowGroup.Key,
                                 Values = columns.GroupJoin(
                                     rowGroup,
                                     c => c,
                                     r => columnSelector(r),
                                     (c, columnGroup) => dataSelector(columnGroup))
                             });

            foreach (var row in rows)
            {
                var dataRow = table.NewRow();
                var items = row.Values.Cast<object>().ToList();
                items.Insert(0, row.Key);
                dataRow.ItemArray = items.ToArray();
                table.Rows.Add(dataRow);
            }

            return table;
        }

        /// <summary>
        /// Hierarchical grouping data using linq
        /// </summary>
        /// <typeparam name="TElement">Объект</typeparam>
        /// <param name="elements">Исходный перечень объектов</param>
        /// <param name="groupSelectors">Перечень функций для группировки данных</param>
        /// <returns></returns>
        public static IEnumerable<GroupResult<TElement>> GroupByMany<TElement>(
            this IEnumerable<TElement> elements,
            params Func<TElement, object>[] groupSelectors)
        {
            Func<IEnumerable<TElement>, IEnumerable<GroupResult<TElement>>> groupBy = source => null;
            for (int i = groupSelectors.Length - 1; i >= 0; i--)
            {
                var keySelector = groupSelectors[i]; // Capture
                var subGroupsSelector = groupBy; // Capture
                groupBy = source => source.GroupBy(keySelector).Select(g => new GroupResult<TElement>
                {
                    Key = g.Key,
                    Count = g.Count(),
                    Items = g,
                    SubGroups = subGroupsSelector(g)
                });
            }
            return groupBy(elements);
        }

        /// <summary>
        /// Класс, представляющий многоуровневую группу, использумый <see cref="GroupByMany"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class GroupResult<T>
        {
            public object Key { get; set; }
            public int Count { get; set; }
            public IEnumerable<T> Items { get; set; }
            public IEnumerable<GroupResult<T>> SubGroups { get; set; }
            public override string ToString() { return string.Format("{0} ({1})", Key, Count); }
        }
    }
}
