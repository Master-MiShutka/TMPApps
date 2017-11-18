using System;
using System.Collections.Generic;
using System.Linq;

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

    }
}
