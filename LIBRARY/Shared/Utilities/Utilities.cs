using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace TMP.Shared
{
    public static partial class U
    {
        #region Members
        /// <summary>
        /// Содержит время первой инициализации класса
        /// </summary>
        public static DateTime initTime;

        private static Dictionary<string, object> locks = new Dictionary<string, object>();
        private static object logLock = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Возвращает / задаёт путь к файлу журнала
        /// </summary>
        public static string LogFile { get; set; }
        /// <summary>
        /// Возвращвет / задаёт минимальный уровень сообщений для печати или записи
        /// </summary>
        public static LogLevel Level { get; set; }
        /// <summary>
        /// Возвращает папку откуда запущен исполняемый файл
        /// </summary>
        public static string BasePath
        {
            get
            {
                return Path.GetDirectoryName(FullPath);
            }
        }
        /// <summary>
        /// Возвращает контекст GUI потока
        /// </summary>
        public static SynchronizationContext GUIContext { get; set; }
        /// <summary>
        /// Возвращает / задаёт путь к исполняемому файлу
        /// </summary>
        public static string FullPath { get; set; }
        #endregion

        #region Constructor
        static U()
        {
            ;
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Инициализация класса
        /// </summary>
        /// <param name="fullPath">Полный путь к исполняемому файлу</param>
        /// <param name="ctx">Контекст синхронизации GUI потока</param>
        /// <param name="logLevel">Минимальный уровень журналирования</param>
        public static void Initialize(string fullPath, SynchronizationContext ctx, LogLevel logLevel = LogLevel.Warning)
        {
            FullPath = fullPath;
            GUIContext = ctx;
            LogFile = Path.Combine(BasePath, "TMPApps.log");
            Level = logLevel;
            initTime = DateTime.Now;
        }
        /// <summary>
        /// Передаёт сообщение в файл или на консоль
        /// </summary>
        /// <param name="level">Уровень сообщения (если меньше чем установленный будет проигнорирован)</param>
        /// <param name="caller">Отправитель</param>
        /// <param name="message">Сообщение</param>
        public static void L(LogLevel level, string caller, string message)
        {
            if (LevelToInt(level) < LevelToInt(Level)) return;

            TimeSpan ts = (DateTime.Now - initTime);
            string logLine = String.Format("{0} {1}:{2:00}:{3:00}.{4:000} ({5:G}) [{6}] {7}: {8}",
                ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds, DateTime.Now,
                LevelToString(level), // #7
                caller.ToUpper(),
                message);

            if (Level == LogLevel.Debug)
                Console.WriteLine(logLine);

#if (!DEBUG)
			lock (logLock)
			{
				System.IO.StreamWriter sw = null;
				try
				{
					sw = System.IO.File.AppendText(LogFile);
					sw.WriteLine(logLine);
				}
				catch (Exception e)
				{
					Console.WriteLine("ERROR: Could not write to logfile: " + e.Message);
				}
				if (sw != null)
					sw.Close();
			}
#endif
        }
        /// <summary>
        /// Передаёт HttpWebResponse в файл или на консоль
        /// </summary>
        /// <param name="level">Уровень сообщения</param>
        /// <param name="caller">Отправитель</param>
        /// <param name="response">Объект HttpWebResponse</param>
        public static void L(LogLevel level, string caller, System.Net.HttpWebResponse response)
        {
            if (response == null)
                U.L(level, caller, "Response was empty.");
            else
            {
                U.L(level, caller, String.Format("Content Encoding: {0}", response.ContentEncoding));
                U.L(level, caller, String.Format("Content Type: {0}", response.ContentType));
                U.L(level, caller, String.Format("Status Description: {0}", response.StatusDescription));
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str;
                while ((str = sr.ReadLine()) != null)
                    U.L(level, caller, str);
                U.L(level, caller, String.Format("-- End of response. Total bytes: {0} --", response.ContentLength));
            }
        }
        /// <summary>
        /// Удаляет лишние символы из строки. Знак _ заменяется на -
        /// </summary>
        /// <param name="str">Строка для очистки</param>
        /// <returns>Строка</returns>
        public static string C(string str)
        {
            if (String.IsNullOrWhiteSpace(str)) return "";
            Regex rgx = new Regex(@"[^\s\w_]");
            return rgx.Replace(str, "-");
        }
        /// <summary>
        /// Перевод строки
        /// </summary>
        /// <param name="id">ИД транслируемого значения</param>
        /// <param name="field">Поле транслируемого значения</param>
        /// <param name="def">Значение по-умолчанию</param>
        /// <returns>Строка, ассоциируемая с транслиуемым значением</returns>
        public static string T(string id, string field = "Text", string def = "")
        {
            if (def == "") def = field;
			//LanguageDictionary dictionary = LanguageDictionary.GetDictionary(LanguageContext.Instance.Culture);
			//return (string)dictionary.Translate(id, field, def, typeof(string));
            return def;
        }
        #region Форматирование значений

        public static string T(uint n)
        {
            if (n == 0)
                return "0";
            return n.ToString("N0", Thread.CurrentThread.CurrentCulture);
        }
        public static string T(int n)
        {
            if (n == 0)
                return "0";
            return n.ToString("N0", Thread.CurrentThread.CurrentCulture);
        }
        public static string T(long n)
        {
            if (n == 0)
                return "0";
            return n.ToString("N0", Thread.CurrentThread.CurrentCulture);
        }
        public static string T(ulong n)
        {
            if (n == 0)
                return "0";
            return n.ToString("N0", Thread.CurrentThread.CurrentCulture);
        }
        public static string T(double n)
        {
            return n.ToString("N", Thread.CurrentThread.CurrentCulture);
        }
        public static string T(DateTime dt)
        {
            if (dt.Year < 2)
                return T("Never", "Text", "Never");
            return dt.ToString(Thread.CurrentThread.CurrentCulture);
        }

        #endregion
        #endregion
        #region #region Private
        private static int LevelToInt(LogLevel level)
        {
            if (level == LogLevel.Debug) return 1;
            else if (level == LogLevel.Information) return 2;
            else if (level == LogLevel.Warning) return 3;
            else if (level == LogLevel.Error) return 4;
            else return 0;
        }
        private static string LevelToString(LogLevel level)
        {
            if (level == LogLevel.Debug) return "DEBUG";
            else if (level == LogLevel.Information) return "INFO";
            else if (level == LogLevel.Warning) return "OOPS";
            else if (level == LogLevel.Error) return "SHIT";
            else return "HUH?";
        }
        #endregion
        #endregion
    }

    #region Enum
    /// <summary>
    /// Описывает уровень сообщений
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error
    }
    #endregion

    #region Data structures
    /// <summary>
    /// Шаблон аргумента события
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The value of the event.
        /// </summary>
        T value;

        /// <summary>
        /// Gets the value of the event.
        /// </summary>
        public T Value { get { return value; } }

        /// <summary>
        /// Creates a generic event data structure.
        /// </summary>
        /// <param name="value">The value of the event data</param>
        public GenericEventArgs(T value) { this.value = value; }
    }
    #endregion
}
