namespace TMP.Shared
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// A generic class used to serialize objects.
    /// </summary>
    public class GenericSerializer
    {
        /// <summary>
        /// Serializes the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>String representation of the serialized object.</returns>
        public static string Serialize<T>(T obj)
        {
            XmlSerializer xs = null;
            StringWriter sw = null;
            try
            {
                xs = new XmlSerializer(typeof(T));
                sw = new StringWriter();
                xs.Serialize(sw, obj);
                sw.Flush();
                return sw.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                }
            }
        }

        public static string Serialize<T>(T obj, Type[] extraTypes)
        {
            XmlSerializer xs = null;
            StringWriter sw = null;
            try
            {
                xs = new XmlSerializer(typeof(T), extraTypes);
                sw = new StringWriter();
                xs.Serialize(sw, obj);
                sw.Flush();
                return sw.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                }
            }
        }

        /// <summary>
        /// Serializes the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="writer">The writer to be used for output in the serialization.</param>
        public static void Serialize<T>(T obj, XmlWriter writer)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            xs.Serialize(writer, obj);
        }

        /// <summary>
        /// Serializes the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="writer">The writer to be used for output in the serialization.</param>
        /// <param name="extraTypes"><c>Type</c> array
        ///       of additional object types to serialize.</param>
        public static void Serialize<T>(T obj, XmlWriter writer, Type[] extraTypes)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T), extraTypes);
            xs.Serialize(writer, obj);
        }

        /// <summary>
        /// Deserializes the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be deserialized.</typeparam>
        /// <param name="reader">The reader used to retrieve the serialized object.</param>
        /// <returns>The deserialized object of type T.</returns>
        public static T Deserialize<T>(XmlReader reader)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            return (T)xs.Deserialize(reader);
        }

        /// <summary>
        /// Deserializes the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be deserialized.</typeparam>
        /// <param name="reader">The reader used to retrieve the serialized object.</param>
        /// <param name="extraTypes"><c>Type</c> array
        ///           of additional object types to deserialize.</param>
        /// <returns>The deserialized object of type T.</returns>
        public static T Deserialize<T>(XmlReader reader, Type[] extraTypes)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T), extraTypes);
            return (T)xs.Deserialize(reader);
        }

        /// <summary>
        /// Deserializes the given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be deserialized.</typeparam>
        /// <param name="xML">The XML file containing the serialized object.</param>
        /// <returns>The deserialized object of type T.</returns>
        public static T Deserialize<T>(string xML)
        {
            if (xML == null || xML == string.Empty)
            {
                return default(T);
            }

            XmlSerializer xs = null;
            StringReader sr = null;
            try
            {
                xs = new XmlSerializer(typeof(T));
                sr = new StringReader(xML);
                return (T)xs.Deserialize(sr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
            }
        }

        public static T Deserialize<T>(string xML, Type[] extraTypes)
        {
            if (xML == null || xML == string.Empty)
            {
                return default(T);
            }

            XmlSerializer xs = null;
            StringReader sr = null;
            try
            {
                xs = new XmlSerializer(typeof(T), extraTypes);
                sr = new StringReader(xML);
                return (T)xs.Deserialize(sr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
            }
        }

        public static void SaveAs<T>(T obj, string FileName,
                           Encoding encoding, Type[] extraTypes)
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(FileName));
            if (!di.Exists)
            {
                di.Create();
            }

            XmlDocument document = new XmlDocument();
            XmlWriterSettings wSettings = new XmlWriterSettings();
            wSettings.Indent = true;
            wSettings.Encoding = encoding;
            wSettings.CloseOutput = true;
            wSettings.CheckCharacters = false;
            using (XmlWriter writer = XmlWriter.Create(FileName, wSettings))
            {
                if (extraTypes != null)
                {
                    GenericSerializer.Serialize<T>(obj, writer, extraTypes);
                }
                else
                {
                    Serialize<T>(obj, writer);
                }

                writer.Flush();
                document.Save(writer);
            }
        }

        public static void SaveAs<T>(T obj, string FileName, Type[] extraTypes)
        {
            SaveAs<T>(obj, FileName, Encoding.UTF8, extraTypes);
        }

        public static void SaveAs<T>(T obj, string FileName, Encoding encoding)
        {
            SaveAs<T>(obj, FileName, encoding, null);
        }

        public static void SaveAs<T>(T obj, string FileName)
        {
            SaveAs<T>(obj, FileName, Encoding.UTF8);
        }

        public static T Open<T>(string fileName, Type[] extraTypes)
        {
            T obj = default(T);
            if (File.Exists(fileName))
            {
                XmlReaderSettings rSettings = new XmlReaderSettings();
                rSettings.CloseInput = true;
                rSettings.CheckCharacters = false;
                using (XmlReader reader = XmlReader.Create(fileName, rSettings))
                {
                    reader.ReadOuterXml();
                    if (extraTypes != null)
                    {
                        obj = Deserialize<T>(reader, extraTypes);
                    }
                    else
                    {
                        obj = Deserialize<T>(reader);
                    }
                }
            }

            return obj;
        }

        public static T Open<T>(string fileName)
        {
            return Open<T>(fileName, null);
        }
    }
}
