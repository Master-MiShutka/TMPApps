namespace TMP.Common.JsonUtils.JsonClassGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TMP.Common.JsonUtils.JsonClassGenerator.CodeWriters;

    public class JsonClassGenerator : IJsonClassGeneratorConfig
    {
        public string Example { get; set; }

        public string TargetFolder { get; set; }

        public string Namespace { get; set; }

        public string SecondaryNamespace { get; set; }

        public bool UseProperties { get; set; }

        public bool InternalVisibility { get; set; }

        public bool ExplicitDeserialization { get; set; }

        public bool NoHelperClass { get; set; }

        public string MainClass { get; set; }

        public bool UsePascalCase { get; set; }

        public bool UseNestedClasses { get; set; }

        public bool ApplyObfuscationAttributes { get; set; }

        public bool SingleFile { get; set; }

        public ICodeWriter CodeWriter { get; set; }

        public TextWriter OutputStream { get; set; }

        public bool AlwaysUseNullableValues { get; set; }

        public bool ExamplesInDocumentation { get; set; }

        private bool used = false;

        public bool UseNamespaces => this.Namespace != null;

        public void GenerateClasses()
        {
            if (this.CodeWriter == null)
            {
                this.CodeWriter = new CSharpCodeWriter();
            }

            if (this.ExplicitDeserialization && !(this.CodeWriter is CSharpCodeWriter))
            {
                throw new ArgumentException("Explicit deserialization is obsolete and is only supported by the C# provider.");
            }

            if (this.used)
            {
                throw new InvalidOperationException("This instance of JsonClassGenerator has already been used. Please create a new instance.");
            }

            this.used = true;

            var writeToDisk = this.TargetFolder != null;
            if (writeToDisk && !Directory.Exists(this.TargetFolder))
            {
                Directory.CreateDirectory(this.TargetFolder);
            }

            JObject[] examples;
            var example = this.Example.StartsWith("HTTP/") ? this.Example.Substring(this.Example.IndexOf("\r\n\r\n")) : this.Example;
            using (var sr = new StringReader(example))
            using (var reader = new JsonTextReader(sr))
            {
                var json = JToken.ReadFrom(reader);
                if (json is JArray)
                {
                    examples = ((JArray)json).Cast<JObject>().ToArray();
                }
                else if (json is JObject)
                {
                    examples = new[] { (JObject)json };
                }
                else
                {
                    throw new Exception("Sample JSON must be either a JSON array, or a JSON object.");
                }
            }

            this.Types = new List<JsonType>();
            this.Names.Add(this.MainClass);
            var rootType = new JsonType(this, examples[0]);
            rootType.IsRoot = true;
            rootType.AssignName(this.MainClass);
            this.GenerateClass(examples, rootType);

            if (writeToDisk)
            {

                var parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (writeToDisk && !this.NoHelperClass && this.ExplicitDeserialization)
                {
                    File.WriteAllBytes(Path.Combine(this.TargetFolder, "JsonClassHelper.cs"), Properties.Resources.JsonClassHelper);
                }

                if (this.SingleFile)
                {
                    this.WriteClassesToFile(Path.Combine(this.TargetFolder, this.MainClass + this.CodeWriter.FileExtension), this.Types);
                }
                else
                {
                    foreach (var type in this.Types)
                    {
                        var folder = this.TargetFolder;
                        if (!this.UseNestedClasses && !type.IsRoot && this.SecondaryNamespace != null)
                        {
                            var s = this.SecondaryNamespace;
                            if (s.StartsWith(this.Namespace + "."))
                            {
                                s = s.Substring(this.Namespace.Length + 1);
                            }

                            folder = Path.Combine(folder, s);
                            Directory.CreateDirectory(folder);
                        }

                        this.WriteClassesToFile(Path.Combine(folder, (this.UseNestedClasses && !type.IsRoot ? this.MainClass + "." : string.Empty) + type.AssignedName + this.CodeWriter.FileExtension), new[] { type });
                    }
                }
            }
            else if (this.OutputStream != null)
            {
                this.WriteClassesToFile(this.OutputStream, this.Types);
            }
        }

        private void WriteClassesToFile(string path, IEnumerable<JsonType> types)
        {
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                this.WriteClassesToFile(sw, types);
            }
        }

        private void WriteClassesToFile(TextWriter sw, IEnumerable<JsonType> types)
        {
            var inNamespace = false;
            var rootNamespace = false;

            this.CodeWriter.WriteFileStart(this, sw);
            foreach (var type in types)
            {
                if (this.UseNamespaces && inNamespace && rootNamespace != type.IsRoot && this.SecondaryNamespace != null)
                {
                    this.CodeWriter.WriteNamespaceEnd(this, sw, rootNamespace);
                    inNamespace = false;
                }

                if (this.UseNamespaces && !inNamespace)
                {
                    this.CodeWriter.WriteNamespaceStart(this, sw, type.IsRoot);
                    inNamespace = true;
                    rootNamespace = type.IsRoot;
                }

                this.CodeWriter.WriteClass(this, sw, type);
            }

            if (this.UseNamespaces && inNamespace)
            {
                this.CodeWriter.WriteNamespaceEnd(this, sw, rootNamespace);
            }

            this.CodeWriter.WriteFileEnd(this, sw);
        }

        private void GenerateClass(JObject[] examples, JsonType type)
        {
            var jsonFields = new Dictionary<string, JsonType>();
            var fieldExamples = new Dictionary<string, IList<object>>();

            var first = true;

            foreach (var obj in examples)
            {
                foreach (var prop in obj.Properties())
                {
                    JsonType fieldType;
                    var currentType = new JsonType(this, prop.Value);
                    var propName = prop.Name;
                    if (jsonFields.TryGetValue(propName, out fieldType))
                    {
                        var commonType = fieldType.GetCommonType(currentType);
                        jsonFields[propName] = commonType;
                    }
                    else
                    {
                        var commonType = currentType;
                        if (first)
                        {
                            commonType = commonType.MaybeMakeNullable(this);
                        }
                        else
                        {
                            commonType = commonType.GetCommonType(JsonType.GetNull(this));
                        }

                        jsonFields.Add(propName, commonType);
                        fieldExamples[propName] = new List<object>();
                    }

                    var fe = fieldExamples[propName];
                    var val = prop.Value;
                    if (val.Type == JTokenType.Null || val.Type == JTokenType.Undefined)
                    {
                        if (!fe.Contains(null))
                        {
                            fe.Insert(0, null);
                        }
                    }
                    else
                    {
                        var v = val.Type == JTokenType.Array || val.Type == JTokenType.Object ? val : val.Value<object>();
                        if (!fe.Any(x => v.Equals(x)))
                        {
                            fe.Add(v);
                        }
                    }
                }

                first = false;
            }

            if (this.UseNestedClasses)
            {
                foreach (var field in jsonFields)
                {
                    this.Names.Add(field.Key.ToLower());
                }
            }

            foreach (var field in jsonFields)
            {
                var fieldType = field.Value;
                if (fieldType.Type == JsonTypeEnum.Object)
                {
                    var subexamples = new List<JObject>(examples.Length);
                    foreach (var obj in examples)
                    {
                        JToken value;
                        if (obj.TryGetValue(field.Key, out value))
                        {
                            if (value.Type == JTokenType.Object)
                            {
                                subexamples.Add((JObject)value);
                            }
                        }
                    }

                    fieldType.AssignName(this.CreateUniqueClassName(field.Key));
                    this.GenerateClass(subexamples.ToArray(), fieldType);
                }

                if (fieldType.InternalType != null && fieldType.InternalType.Type == JsonTypeEnum.Object)
                {
                    var subexamples = new List<JObject>(examples.Length);
                    foreach (var obj in examples)
                    {
                        JToken value;
                        if (obj.TryGetValue(field.Key, out value))
                        {
                            if (value.Type == JTokenType.Array)
                            {
                                foreach (var item in (JArray)value)
                                {
                                    if (!(item is JObject))
                                    {
                                        throw new NotSupportedException("Arrays of non-objects are not supported yet.");
                                    }

                                    subexamples.Add((JObject)item);
                                }
                            }
                            else if (value.Type == JTokenType.Object)
                            {
                                foreach (var item in (JObject)value)
                                {
                                    if (!(item.Value is JObject))
                                    {
                                        throw new NotSupportedException("Arrays of non-objects are not supported yet.");
                                    }

                                    subexamples.Add((JObject)item.Value);
                                }
                            }
                        }
                    }

                    field.Value.InternalType.AssignName(this.CreateUniqueClassNameFromPlural(field.Key));
                    this.GenerateClass(subexamples.ToArray(), field.Value.InternalType);
                }
            }

            type.Fields = jsonFields.Select(x => new FieldInfo(this, x.Key, x.Value, this.UsePascalCase, fieldExamples[x.Key])).ToArray();
            this.Types.Add(type);
        }

        public IList<JsonType> Types { get; private set; }

        private HashSet<string> Names = new HashSet<string>();

        private string CreateUniqueClassName(string name)
        {
            name = ToTitleCase(name);

            var finalName = name;
            var i = 2;
            while (this.Names.Any(x => x.Equals(finalName, StringComparison.OrdinalIgnoreCase)))
            {
                finalName = name + i.ToString();
                i++;
            }

            this.Names.Add(finalName);
            return finalName;
        }

        private string CreateUniqueClassNameFromPlural(string plural)
        {
            plural = ToTitleCase(plural);
            return this.CreateUniqueClassName(plural);
        }

        internal static string ToTitleCase(string str)
        {
            var sb = new StringBuilder(str.Length);
            var flag = true;

            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(flag ? char.ToUpper(c) : c);
                    flag = false;
                }
                else
                {
                    flag = true;
                }
            }

            return sb.ToString();
        }

        public bool HasSecondaryClasses => this.Types.Count > 1;
    }
}