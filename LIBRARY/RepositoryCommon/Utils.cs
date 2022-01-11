namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class Utils
    {
        public static string GetExceptionDetails(Exception exp)
        {
            string message = string.Empty;
            if (exp is AggregateException)
            {
                AggregateException ae = exp as AggregateException;
                foreach (var e in ae.InnerExceptions)
                {
                    message += Environment.NewLine + e.Message + Environment.NewLine;
                }
            }
            else
            {
                try
                {
                    // Write Message tree of inner exception into textual representation
                    message = exp.Message;

                    Exception innerEx = exp.InnerException;

                    for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                    {
                        string spaces = string.Empty;

                        for (int j = 0; j < i; j++)
                        {
                            spaces += "  ";
                        }

                        message += "\n" + spaces + "└─>" + innerEx.Message;
                    }
                }
                catch
                {
                }
            }

            return message;
        }

        /// <summary>
        /// Путь к папке с данными программы
        /// </summary>
        public static string AppDataSettingsPath
        {
            get
            {
                // Получение сборки приложения
                var assm = System.Reflection.Assembly.GetEntryAssembly();
                var at = typeof(System.Reflection.AssemblyCompanyAttribute);
                object[] customAttributes = null;
                try
                {
                    // Получение из метаданных коллекции аттрибутов
                    customAttributes = assm.GetCustomAttributes(at, false);
                }
                catch (Exception)
                {
                }

                // Получения из метаданных аттрибута компания
                System.Reflection.AssemblyCompanyAttribute ct =
                              (System.Reflection.AssemblyCompanyAttribute)customAttributes[0];

                // получение пути к данным программы в папке пользователя
                if (ct != null)
                {
                    return System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        ct.Company,
                        assm.GetName().Name,
                        assm.GetName().Version.ToString());
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Путь к папке с исполняемым файлом программы
        /// </summary>
        public static string ExecutionPath => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    }
}
