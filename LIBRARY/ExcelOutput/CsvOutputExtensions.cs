using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using TMP.Extensions;

namespace TMP.ExcelOutput
{
    public static class CsvOutputExtensions
    {
        /// <summary>
        ///     Returns a csv string using all of the public properties and public fields from each
        ///     item of type <see cref="T" /> in the <see cref="IEnumerable{T}" /> collection using the default separator.
        /// </summary>
        /// <typeparam name="T">Any type T.</typeparam>
        /// <param name="input">An <see cref="IEnumerable{T}" />.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToCsv<T>(this IEnumerable<T> input)
        {
            return input.ToCsv(null, ',', false);
        }

        /// <summary>
        ///     Returns a csv string using all of the public properties and public fields from each
        ///     item of type <see cref="T" /> in the <see cref="IEnumerable{T}" /> collection using the default separator.
        /// </summary>
        /// <typeparam name="T">Any type T.</typeparam>
        /// <param name="input">An <see cref="IEnumerable{T}" />.</param>
        /// <param name="trim">A parameter which indicates whether the strings for each element should be trimmed or not.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToCsv<T>(this IEnumerable<T> input, bool trim)
        {
            return input.ToCsv(null, ',', trim);
        }

        /// <summary>
        ///     Returns a csv string using all of the public properties and public fields from each
        ///     item of Type T in the IEnumerable collection using the specified <see cref="separator" />
        ///     character.
        /// </summary>
        /// <typeparam name="T">Any type T.</typeparam>
        /// <param name="input">An <see cref="IEnumerable{T}" />.</param>
        /// <param name="separator">A separator to use for the csv output.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToCsv<T>(this IEnumerable<T> input, char separator)
        {
            return input.ToCsv(null, separator, false);
        }

        /// <summary>
        ///     Returns a csv string using all of the public properties and public fields from each
        ///     item of Type T in the IEnumerable collection using the specified <see cref="separator" />
        ///     character.
        /// </summary>
        /// <typeparam name="T">Any type T.</typeparam>
        /// <param name="input">An <see cref="IEnumerable{T}" />.</param>
        /// <param name="separator">A separator to use for the csv output.</param>
        /// <param name="trim">A parameter which indicates whether the strings for each element should be trimmed or not.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToCsv<T>(this IEnumerable<T> input, char separator, bool trim)
        {
            return input.ToCsv(null, separator, trim);
        }

        /// <summary>
        ///     Returns a csv string using all of the public properties and public fields from each
        ///     item of Type T in the IEnumerable collection using the default separator ',' and saves
        ///     the generated string to <see cref="path" />.
        /// </summary>
        /// <typeparam name="T">Any type T.</typeparam>
        /// <param name="input">An <see cref="IEnumerable{T}" />.</param>
        /// <param name="path">A path string which is used to automatically write the generated csv to a file.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToCsv<T>(this IEnumerable<T> input, string path)
        {
            return input.ToCsv(path, ',', false);
        }

        /// <summary>
        ///     Returns a csv string using all of the public properties and public fields from each
        ///     item of Type T in the IEnumerable collection using the default separator ',' and saves
        ///     the generated string to <see cref="path" />.
        /// </summary>
        /// <typeparam name="T">Any type T.</typeparam>
        /// <param name="input">An <see cref="IEnumerable{T}" />.</param>
        /// <param name="path">A path string which is used to automatically write the generated csv to a file.</param>
        /// <param name="trim">A parameter which indicates whether the strings for each element should be trimmed or not.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToCsv<T>(this IEnumerable<T> input, string path, bool trim)
        {
            return input.ToCsv(path, ',', trim);
        }

        /// <summary>
        ///     Returns a csv string using all of the public properties and public fields from each
        ///     item of Type T in the IEnumerable collection using the specified <see cref="separator" />
        ///     character and saves the generated string to <see cref="path" />.
        /// </summary>
        /// <typeparam name="T">Any type T.</typeparam>
        /// <param name="input">An <see cref="IEnumerable{T}" />.</param>
        /// <param name="path">A path string which is used to automatically write the generated csv to a file.</param>
        /// <param name="separator">A separator to use for the csv output.</param>
        /// <param name="trim">A parameter which indicates whether the strings for each element should be trimmed or not.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToCsv<T>(this IEnumerable<T> input, string path, char separator, bool trim)
        {
            var type = typeof(T);
            string output;

            if (trim)
            {
                if (type.IsSimpleType())
                    output = string.Join(separator.ToTrimmedString(), input);

                else
                {
                    var properties = type.GetProperties();
                    var fields = type.GetFields();
                    var stringBuilder = new StringBuilder();

                    stringBuilder.AppendLine(
                                             string.Concat(
                                                           string.Join(separator.ToString(), properties.Where(p => p.PropertyType.IsSimpleType()).Select(p => p.GetCustomAttribute<ColumnHeaderAttribute>() != null ? p.GetCustomAttribute<ColumnHeaderAttribute>().Header : p.Name.ToTrimmedString()).ToArray()),
                                                           fields.Any(f => f.FieldType.IsSimpleType()) ? separator.ToString() : string.Empty,
                                                           string.Join(separator.ToString(), fields.Where(f => f.FieldType.IsSimpleType()).Select(f => f.Name.ToTrimmedString()).ToArray())
                                                 )
                        );

                    foreach (var element in input)
                    {
                        stringBuilder.AppendLine(
                                                 string.Concat(
                                                               string.Join(separator.ToString(), properties.Where(p => p.PropertyType.IsSimpleType()).Select(p => p.GetValue(element, null).ToTrimmedString()).ToArray()),
                                                               fields.Any(f => f.FieldType.IsSimpleType()) ? separator.ToString() : string.Empty,
                                                               string.Join(separator.ToString(), fields.Where(f => f.FieldType.IsSimpleType()).Select(f => f.GetValue(element).ToTrimmedString()).ToArray())
                                                     )
                            );
                    }

                    output = stringBuilder.ToString();
                }
            }
            else
            {
                if (type.IsSimpleType())
                    output = string.Join(separator.ToString(), input);

                else
                {
                    var properties = type.GetProperties();
                    var fields = type.GetFields();
                    var stringBuilder = new StringBuilder();

                    stringBuilder.AppendLine(
                                             string.Concat(
                                                           string.Join(separator.ToString(), properties.Where(p => p.PropertyType.IsSimpleType()).Select(p => p.GetCustomAttribute<ColumnHeaderAttribute>() != null ? p.GetCustomAttribute<ColumnHeaderAttribute>().Header : p.Name).ToArray()),
                                                           fields.Any(f => f.FieldType.IsSimpleType()) ? separator.ToString() : string.Empty,
                                                           string.Join(separator.ToString(), fields.Where(f => f.FieldType.IsSimpleType()).Select(f => f.Name).ToArray())
                                                 )
                        );

                    foreach (var element in input)
                    {
                        stringBuilder.AppendLine(
                                                 string.Concat(
                                                               string.Join(separator.ToString(), properties.Where(p => p.PropertyType.IsSimpleType()).Select(p => p.GetValue(element, null)).ToArray()),
                                                               fields.Any(f => f.FieldType.IsSimpleType()) ? separator.ToString() : string.Empty,
                                                               string.Join(separator.ToString(), fields.Where(f => f.FieldType.IsSimpleType()).Select(f => f.GetValue(element)).ToArray())
                                                     )
                            );
                    }

                    output = stringBuilder.ToString();
                }
            }

            if (!string.IsNullOrEmpty(path))
                WriteToPath(output, path);

            return output;
        }

        private static void WriteToPath(string outputString, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(outputString);
                    streamWriter.Flush();
                }
            }
        }
    }
}
