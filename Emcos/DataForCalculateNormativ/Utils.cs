using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public class Utils
    {
        public static NameValueCollection ParsePairs(string data)
        {
            if (String.IsNullOrWhiteSpace(data)) return null;
            var result = new NameValueCollection();

            // записи вида ключ=значение разделены символом &
            var records = data.Split(new char[] { '&' });
            // по всем записям
            for (int i = 0; i < records.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(records[i])) // пустая запись
                    continue;
                var kvp = ParseKeyValuePair(records[i]);
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }

        public static List<NameValueCollection> ParseRecords(string data)
        {
            if (String.IsNullOrWhiteSpace(data)) return null;

            // записи вида ключ=значение разделены символом &
            var records = data.Split(new char[] { '&' });

            // записи содержат результат выполнения запроса (result=0 или result=?) и количество записей (recordCount=N)
            bool resultok = false;
            // запись с количеством
            var recCount = "";
            foreach (string record in records)
            {
                if (record == "result=0")
                    resultok = true;
                if (record.StartsWith("recordCount"))
                    recCount = record;
            }
            if (resultok == false)
                return null;
            int recordsCount = 0;
            // количество записей
            Int32.TryParse(ParseKeyValuePair(recCount).Value, out recordsCount);

            // ключ каждой записи имеет окончание вида _?, где ? указывает на порядковый номер объекта

            // список объектов, каждый из которых имеет список записей в виде пар ключ-значение
            var result = new List<NameValueCollection>();
            int index = 0;
            // по всем записям
            for (int i = 0; i < recordsCount; i++)
            {
                // список пар ключ-значение текущего объекта
                var list = new NameValueCollection();
                while (index < records.Length)
                {
                    if (String.IsNullOrWhiteSpace(records[index]) // пустая запись
                        || records[index].StartsWith("result")    // результат выполнения
                        || records[index].StartsWith("record"))  // количество записей
                    {
                        index++;
                        continue;
                    }
                    // порядковый номер текущего объекта
                    var currentRecordIndexAsString = i.ToString();
                    // пара ключ-значение из текущей записи
                    var kvp = ParseKeyValuePair(records[index]);
                    // если пара относится к текущему объекту
                    if (kvp.Key.EndsWith(currentRecordIndexAsString) == true)
                    {
                        // убираем окончание _?, где ? число
                        var key = kvp.Key;
                        int lastind = key.LastIndexOf("_");
                        key = key.Remove(lastind);

                        // добавляем в список
                        list.Add(key, kvp.Value);
                        index++;
                    }
                    else
                        break;
                }
                result.Add(list);
            }
            return result;
        }

        public static KeyValuePair<string, string> ParseKeyValuePair(string data)
        {
            var parts = data.Split(new char[] { '=' });
            if (parts.Length != 2) throw new ArgumentException();

            return new KeyValuePair<string, string>(parts[0], parts[1]);
        }
    }
}