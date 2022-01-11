using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace FixSavedSessions
{
    using TMP.Work.Emcos.Model;
    using TMP.Work.Emcos.Model.Balans;
    class Program
    {
        public static string SESSIONS_FOLDER;
        public static string SESSION_FILE_EXTENSION;
        static void Main(string[] args)
        {
            TMP.Work.Emcos.ViewModel.BalanceViewModel vm = new TMP.Work.Emcos.ViewModel.BalanceViewModel(null);

            SESSIONS_FOLDER = String.IsNullOrEmpty(vm.SESSIONS_FOLDER) ? "Sessions" : vm.SESSIONS_FOLDER;
            SESSION_FILE_EXTENSION = String.IsNullOrEmpty(vm.SESSION_FILE_EXTENSION) ? ".session-data" : vm.SESSION_FILE_EXTENSION;

            string outFolder = SESSIONS_FOLDER + @"\out";
            if (System.IO.Directory.Exists(outFolder) == false)
                System.IO.Directory.CreateDirectory(outFolder);
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(SESSIONS_FOLDER);
                if (di.Exists == false)
                {
                    throw new FileNotFoundException("Папка '" + SESSIONS_FOLDER + "' не найдена!");
                }
                else
                    Console.WriteLine("Поиск файлов сессий програмы EmcosSiteWrapper в папке '" + di.FullName + "'.");
                FileInfo[] files = di.GetFiles("*"+SESSION_FILE_EXTENSION).OrderByDescending(p => p.LastWriteTime).ToArray();
                Console.WriteLine("Найдено " + files.Count() + " сессий.");
                foreach (FileInfo fi in files)
                    if (fi.Extension == SESSION_FILE_EXTENSION)
                    {
                        Console.Write("[Сессия - " + fi.Name + "] ");
                        try
                        {
                            bool isOldVersion = false;
                            DateTime? start = new Nullable<DateTime>(), end = new Nullable<DateTime>();
                            DatePeriod period = null;

                            BalansSession session = null;

                            using (FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                            {
                                string gzFileSignature = "1F 8B 08";
                                byte[] fileSignature = new byte[3];
                                int readed = fs.Read(fileSignature, 0, 3);
                                if (readed == 3)
                                {
                                    string hex = BitConverter.ToString(fileSignature).Replace("-", " ");
                                    if (hex == gzFileSignature)
                                    {
                                        Console.Write("\t* это gz архив");
                                    }
                                }
                                else
                                {

                                }
                                fs.Seek(0, SeekOrigin.Begin);

                                char first = (char)0;
                                using (System.IO.Compression.GZipStream temp = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
                                {
                                    first = (char)temp.ReadByte();
                                    fs.Seek(0, SeekOrigin.Begin);

                                    using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
                                    {
                                        if (first == '<')
                                        {
                                            Console.WriteLine("\t* это XML");
                                            #region Parse XML
                                            using (XmlReader xmlReader = XmlReader.Create(gzip))
                                            {
                                                XmlDocument xdoc = new XmlDocument();
                                                xdoc.Load(xmlReader);
                                                XmlNamespaceManager nsmanager = new XmlNamespaceManager(xdoc.NameTable);
                                                nsmanager.AddNamespace("x", "http://tmp.work.balans-substations.com");


                                                XmlNode node = xdoc.DocumentElement.SelectSingleNode("x:Period", nsmanager);
                                                if (node == null)
                                                // старая версия файла?
                                                {
                                                    isOldVersion = true;
                                                    node = xdoc.DocumentElement.SelectSingleNode("x:StartDate", nsmanager);
                                                    if (node != null && String.IsNullOrWhiteSpace(node.InnerText) == false)
                                                    {
                                                        DateTime date;
                                                        DateTime.TryParse(node.InnerText, out date);
                                                        start = date;
                                                    }
                                                    else
                                                        start = null;

                                                    node = xdoc.DocumentElement.SelectSingleNode("x:EndDate", nsmanager);
                                                    if (node != null && String.IsNullOrWhiteSpace(node.InnerText) == false)
                                                    {
                                                        DateTime date;
                                                        DateTime.TryParse(node.InnerText, out date);
                                                        end = date;
                                                    }
                                                    period = new DatePeriod(start, end);
                                                    Console.WriteLine("\t* это старая версия сессии");
                                                }
                                                else
                                                    Console.WriteLine("\t* это новая версия сессии");
                                            }
                                            #endregion
                                            session = TMP.Common.RepositoryCommon.BaseDeserializer<BalansSession>.GzXmlDataContractDeSerialize(
                                                fi.FullName,
                                                new Type[]
                                                {
                                                typeof(Substation),
                                                typeof(SubstationSection),
                                                typeof(SubstationPowerTransformers),
                                                typeof(SubstationAuxiliary),
                                                typeof(GroupItem),
                                                typeof(BalanceItem),
                                                typeof(Fider),
                                                typeof(PowerTransformer),
                                                typeof(UnitTransformer),
                                                typeof(UnitTransformerBus),
                                                typeof(DatePeriod)
                                                },
                                                OnError);
                                            if (isOldVersion)
                                                session.Period = period;
                                        }
                                        else
                                            if (first == '{')
                                        {
                                            Console.WriteLine("\t* это JSON");
                                            session = TMP.Common.RepositoryCommon.BaseDeserializer<BalansSession>.JsonDeSerializeFromStream(gzip, OnError);

                                        }
                                        else
                                        {
                                            Console.WriteLine("\t* неизвестный файл");
                                        }
                                    }
                                }
                                if (session != null)
                                    TMP.Common.RepositoryCommon.BaseDeserializer<BalansSession>.GzJsonSerialize(
                                        session,
                                        outFolder + @"\" + session.Period.GetFileNameForSaveSession() + vm.SESSION_FILE_EXTENSION);
                            }
                        }
                        catch (Exception e)
                        {
                            OnError(e);
                        }
                    }
            }
            catch (IOException ioe)
            {
                OnError(ioe);
            }
            Console.WriteLine();
            Console.WriteLine("Готово!");
            Console.WriteLine();
            Console.ReadKey();
        }
        static void OnError(Exception e)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.Beep();
            Console.WriteLine("*".PadLeft(20, '*'));
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("*".PadLeft(20, '*'));
        }
    }
}
