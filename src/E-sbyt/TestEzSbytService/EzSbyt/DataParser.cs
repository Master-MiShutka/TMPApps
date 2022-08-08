using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace TMP.Work.AmperM.TestApp.EzSbyt
{
    public class DataParser
    {
        public static Type ObjectType { get; private set; }

        public static IList CreateListOfObjectsFromValueTable(string valueTable)
        {
            if (valueTable.StartsWith(@"<ValueTable") == false)
            {
                App.Log.Log("ResultViewerViewModel-InitSource data is not ValueTable, first 100 chars: " + valueTable.Substring(100));
                return null;
            }
            XDocument xdoc = XDocument.Parse(valueTable);
            XNamespace ns = "http://v8.1c.ru/8.1/data/core";
            XNamespace ns_schema = "http://www.w3.org/2001/XMLSchema";
            XNamespace ns_xs = "xs";
            // разбор первой части ValueTable - описания полей
            // список полей
            IEnumerable<Shared.ObjectsBuilder.Field> fields;
            #region | формирование списка полей |
            fields = xdoc.Root.Descendants(ns + "column")
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

                        Type typeofvalue;
                        switch (valuetype)
                        {
                            case "string":
                                typeofvalue = typeof(string);
                                break;
                            case "decimal":
                                typeofvalue = typeof(decimal);
                                break;
                            case "boolean":
                                typeofvalue = typeof(bool);
                                break;
                            case "datetime":
                                typeofvalue = typeof(DateTime);
                                break;
                            default:
                                typeofvalue = typeof(string);
                                break;
                        }
                        return new Shared.ObjectsBuilder.Field
                        {
                            FieldName = c.Element(ns + "Name").Value,
                            FieldType = typeofvalue
                        };
                    }
                      ).ToList<Shared.ObjectsBuilder.Field>();
            #endregion

            // создание динамического типа на основании списка полей
            Type t = Shared.ObjectsBuilder.ObjectBuilder.CreateNewObject(fields);
            ObjectType = t;

            // разбор воторой части ValueTable - строк с данными
            // создание из строк ValueTable списка объектов созданного выше типа
            IList objList = Shared.ObjectsBuilder.ObjectBuilder.GetObjectsList(t);

            #region | разбор строк |
            try
            {
                var rows = xdoc.Root.Descendants(ns + "row").Select(c => new { ValuesAsString = c.Elements(ns + "Value").Select(x => x.Value).ToList() }).ToList();
                foreach (var r in rows)
                {
                    int i = 0;
                    object o = Activator.CreateInstance(t);

                    foreach (var f in fields)
                    {
                        PropertyInfo pInfo = t.GetProperty(f.FieldName);
                        if (pInfo != null)
                            switch (Type.GetTypeCode(f.FieldType))
                            {
                                case TypeCode.Decimal:
                                    decimal d = 0;
                                    Decimal.TryParse(r.ValuesAsString[i++], out d);
                                    pInfo.SetValue(o, d, null);
                                    break;
                                case TypeCode.Boolean:
                                    bool b = false;
                                    Boolean.TryParse(r.ValuesAsString[i++], out b);
                                    pInfo.SetValue(o, b, null);
                                    break;
                                case TypeCode.DateTime:
                                    DateTime dt = default(DateTime);
                                    DateTime.TryParse(r.ValuesAsString[i++], out dt);
                                    pInfo.SetValue(o, dt, null);
                                    break;
                                case TypeCode.Int32:
                                    Int32 int32 = default(Int32);
                                    Int32.TryParse(r.ValuesAsString[i++], out int32);
                                    pInfo.SetValue(o, int32, null);
                                    break;
                                case TypeCode.Int64:
                                    Int64 int64 = default(Int64);
                                    Int64.TryParse(r.ValuesAsString[i++], out int64);
                                    pInfo.SetValue(o, int64, null);
                                    break;
                                default:
                                    pInfo.SetValue(o, r.ValuesAsString[i++], null);
                                    break;
                            }
                        else
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }
                    objList.Add(o);
                }
            }
            catch (Exception ex)
            {
                App.ToLogException(ex);
            }
            #endregion
            return objList;
        }

        public static IList CreateListOfObjectsFromJson(string json)
        {

            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer()
            {
                MaxJsonLength = int.MaxValue
            };
            var d = jss.Deserialize<dynamic>(json);

            // список полей
            List<Shared.ObjectsBuilder.Field> fields = new List<Shared.ObjectsBuilder.Field>();
            #region | формирование списка полей |
            Dictionary<string, Type> keys = new Dictionary<string, Type>();
            foreach (Dictionary<string, object> item in d)
            {
                foreach (string key in item.Keys)
                {
                    Type fieldType = item[key].GetType();
                    if (keys.ContainsKey(key) == false)
                    {
                        keys.Add(key, fieldType);
                        fields.Add(new Shared.ObjectsBuilder.Field() { FieldName = key, FieldType = fieldType });
                    }
                    else
                    {
                        if (fieldType == typeof(Decimal) && (keys[key] == typeof(Int32) || keys[key] == typeof(Int16) || keys[key] == typeof(Int64)))
                        {
                            keys[key] = fieldType;
                            fields.Where(i => i.FieldName == key).First().FieldType = fieldType;
                        }
                    }
                }
            }
            #endregion

            // создание динамического типа на основании списка полей
            Type t = Shared.ObjectsBuilder.ObjectBuilder.CreateNewObject(fields);
            ObjectType = t;

            Type listType = typeof(List<>).MakeGenericType(t);
            IList objList = Shared.ObjectsBuilder.ObjectBuilder.GetObjectsList(t);

            StringBuilder errors = new StringBuilder();

            foreach (Dictionary<string, object> item in d)
            {
                object o = Activator.CreateInstance(t);
                foreach (var field in fields)
                {
                    PropertyInfo pInfo = t.GetProperty(field.FieldName);
                    var value = item[field.FieldName];
                    if (pInfo != null && value != null)
                    {
                        if (String.Equals(value, String.Empty))
                            continue;
                        // если тип значения и свойства не совпадают, пробуем преобразовать
                        if (pInfo.PropertyType != value.GetType())
                        {
                            try
                            {
                                value = System.Convert.ChangeType(value, pInfo.PropertyType);
                                pInfo.SetValue(o, value, null);
                            }
                            catch (Exception ex)
                            {
                                errors.AppendFormat("свойство:'{0}', тип свойства:'{1}', тип значения:'{2}', значение:'{3}'\n",
                                    field.FieldName,
                                    field.FieldType.Name,
                                    value.GetType(),
                                    value);
                                App.ToLogException(ex);
                            }
                        }
                        else
                            pInfo.SetValue(o, value, null);
                    }
                    else
                        System.Diagnostics.Debugger.Break();
                }
                objList.Add(o);
            }

            if (errors.Length > 0)
            {
                App.UIAction(() =>
                {
                    TaskDialog dialog = new TaskDialog();
                    dialog.Caption = "Ошибка приложения";
                    dialog.InstructionText = "Обратите внимание, некоторые значения полей потеряны!";
                    dialog.Text = "При обработке результата не удалось преобразовать некоторые значения.";
                    dialog.Icon = TaskDialogStandardIcon.Warning;
                    dialog.Cancelable = false;

                    dialog.DetailsExpanded = false;
                    dialog.DetailsCollapsedLabel = "Показать ошибки";
                    dialog.DetailsExpandedLabel = "Скрыть";
                    dialog.DetailsExpandedText = errors.ToString();
                    dialog.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandContent;

                    dialog.Show();
                });
            }

            return objList;
        }
    }
}
