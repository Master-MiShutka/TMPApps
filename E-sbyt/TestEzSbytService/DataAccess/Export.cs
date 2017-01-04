using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Xml.Linq;
using System.Data;
using System.Reflection;
using OfficeOpenXml;

namespace TMP.Work.AmperM.TestApp.DataAccess
{
    public class Export
    {
        #region Fields

        #endregion

        #region Constructors



        #endregion

        #region Properties



        #endregion

        #region Public Methods

        public static bool? CreateExcelWorkBookFromCollection(IEnumerable source, Type itemType,
          bool printHeaders = true, string bookFileName = null)
        {
            if (source == null) return null;
            try
            {
                if (String.IsNullOrEmpty(bookFileName))
                {
                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.DefaultExt = ".xml";
                    sfd.Filter = "Электронная таблица (*.xlsx)|*.xlsx";
                    sfd.FilterIndex = 0;
                    sfd.Title = "Сохранение данных";
                    if (sfd.ShowDialog() == true)
                        bookFileName = sfd.FileName;
                    else
                        return null;
                }

                MemberInfo[] Members = itemType.GetProperties();

                System.IO.FileInfo fi = new System.IO.FileInfo(bookFileName);
                using (ExcelPackage pck = new ExcelPackage())
                {
                    OfficeOpenXml.ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Export");
                    ws.Cells["A1"].LoadFromCollection(source.Cast<dynamic>(), printHeaders, OfficeOpenXml.Table.TableStyles.Medium16, BindingFlags.Public, Members);
                    pck.SaveAs(fi);
                }
            }
            catch (Exception e)
            {
                App.ToLogException(e);
                return false;
            }
            return true;
        }

        public static DataTable ToDataTableFromJson(string json)
        {
            if (String.IsNullOrEmpty(json)) return null;
            DataTable result = new DataTable();
            Newtonsoft.Json.Linq.JArray jArray = Newtonsoft.Json.Linq.JArray.Parse(json);
            //Initialize the columns, If you know the row type, replace this   
            foreach (var row in jArray)
            {
                foreach (var jToken in row)
                {
                    Newtonsoft.Json.Linq.JProperty jproperty = jToken as Newtonsoft.Json.Linq.JProperty;
                    if (jproperty == null) continue;
                    if (result.Columns[jproperty.Name] == null)
                        result.Columns.Add(jproperty.Name, typeof(string));
                }
            }
            foreach (var row in jArray)
            {
                var datarow = result.NewRow();
                foreach (var jToken in row)
                {
                    Newtonsoft.Json.Linq.JProperty jProperty = jToken as Newtonsoft.Json.Linq.JProperty;
                    if (jProperty == null) continue;
                    datarow[jProperty.Name] = jProperty.Value.ToString();
                }
                result.Rows.Add(datarow);
            }

            return result;
        }

        public static DataTable ToDataTableFromValueTable(string data)
        {
            if (String.IsNullOrEmpty(data)) return null;
            if (data.StartsWith(@"<ValueTable") == false) return null;

            XDocument xdoc = XDocument.Parse(data);
            XNamespace ns = "http://v8.1c.ru/8.1/data/core";
            var columns = xdoc.Root.Descendants(ns + "column")
              .Select(c =>
              {
                  string valuetype = "string";
                  var types = c.Element(ns + "ValueType").Elements(ns + "Type");
                  if (types != null)
                  {
                      var xstypes = types.Where(x => x.Value.StartsWith("xs:"));
                      if (xstypes != null && xstypes.Count() == 1)
                      {
                          string value = xstypes.First().Value;
                          valuetype = value.Substring(3);
                      }
                  }
                  return new
                  {
                      Name = c.Element(ns + "Name").Value,
                      ValueType = valuetype
                  };
              }
                ).ToList();
            var rows = xdoc.Root.Descendants(ns + "row").Select(c => new { ValuesAsString = c.Elements(ns + "Value").Select(x => x.Value).ToList() }).ToList();

            System.Data.DataTable table = new System.Data.DataTable();
            foreach (var c in columns)
                table.Columns.Add(c.Name);
            foreach (var r in rows)
            {
                System.Data.DataRow datarow = table.NewRow();
                int colindex = 0;
                foreach (var v in r.ValuesAsString)
                    datarow[colindex++] = v;
                table.Rows.Add(datarow);
            }
            return table;
        }

        #endregion

        #region Private Helpers

        private static readonly char[] _letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
        private static string GetColumnLetter(int column)
        {
            if (column <= _letters.Length)
            {
                return _letters[column - 1].ToString();
            }

            var number = column;
            string letter = string.Empty;

            while (number > 0)
            {
                var remainder = (number - 1) % _letters.Length;
                letter = _letters[remainder] + letter;
                number = (number - remainder) / _letters.Length;
            }
            return letter;
        }

        #endregion
    }
}