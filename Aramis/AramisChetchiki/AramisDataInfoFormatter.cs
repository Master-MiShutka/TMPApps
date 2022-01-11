namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using MessagePack;
    using MessagePack.Formatters;
    using TMP.Common.RepositoryCommon;
    using TMP.WORK.AramisChetchiki.Model;

    internal class IDataFileInfoFormatter : IMessagePackFormatter<IDataFileInfo>
    {
        public IDataFileInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);

            Version version = new (1, 1);
            DatePeriod period = null;
            string departamentName = null;
            string aramisDbPath = null;

            // Loop over *all* array elements independently of how many we expect,
            // since if we're serializing an older/newer version of this object it might
            // vary in number of elements that were serialized, but the contract of the formatter
            // is that exactly one data structure must be read, regardless.
            // Alternatively, we could check that the size of the array/map is what we expect
            // and throw if it is not.
            int count = reader.ReadArrayHeader();
            for (int i = 0; i < count; i++)
            {
                switch (i)
                {
                    case 0:
                        _ = Version.TryParse(reader.ReadString(), out version);
                        break;
                    case 1:
                        period = DatePeriod.InitFromString(reader.ReadString());
                        break;
                    case 2:
                        departamentName = reader.ReadString();
                        break;
                    case 3:
                        aramisDbPath = reader.ReadString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return new AramisDataInfo() { Version = version, Period = period, DepartamentName = departamentName, AramisDbPath = aramisDbPath };
        }

        public void Serialize(ref MessagePackWriter writer, IDataFileInfo value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            AramisDataInfo aramisDataInfo = value as AramisDataInfo;
            if (aramisDataInfo == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(4);

            byte[] getBytes(string data)
            {
                return System.Text.Encoding.UTF8.GetBytes(data);
            }

            writer.WriteString(getBytes(aramisDataInfo.Version.ToString()));
            writer.WriteString(getBytes(aramisDataInfo.Period.ToString()));
            writer.WriteString(getBytes(aramisDataInfo.DepartamentName));
            writer.WriteString(getBytes(aramisDataInfo.AramisDbPath));
        }
    }

    internal class AramisDataInfoFormatter : IMessagePackFormatter<AramisDataInfo>
    {
        public AramisDataInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);

            Version version = new (1, 1);
            DatePeriod period = null;
            string departamentName = null;
            string aramisDbPath = null;

            // Loop over *all* array elements independently of how many we expect,
            // since if we're serializing an older/newer version of this object it might
            // vary in number of elements that were serialized, but the contract of the formatter
            // is that exactly one data structure must be read, regardless.
            // Alternatively, we could check that the size of the array/map is what we expect
            // and throw if it is not.
            int count = reader.ReadArrayHeader();
            for (int i = 0; i < count; i++)
            {
                switch (i)
                {
                    case 0:
                        _ = Version.TryParse(reader.ReadString(), out version);
                        break;
                    case 1:
                        period = DatePeriod.InitFromString(reader.ReadString());
                        break;
                    case 2:
                        departamentName = reader.ReadString();
                        break;
                    case 3:
                        aramisDbPath = reader.ReadString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return new AramisDataInfo() { Version = version, Period = period, DepartamentName = departamentName, AramisDbPath = aramisDbPath };
        }

        public void Serialize(ref MessagePackWriter writer, AramisDataInfo value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(4);

            byte[] getBytes(string data)
            {
                return System.Text.Encoding.UTF8.GetBytes(data);
            }

            writer.WriteString(getBytes(value.Version.ToString()));
            writer.WriteString(getBytes(value.Period.ToString()));
            writer.WriteString(getBytes(value.DepartamentName));
            writer.WriteString(getBytes(value.AramisDbPath));
        }
    }

    public class AramisDataInfoResolver : IFormatterResolver
    {
        // Resolver should be singleton.
        public static readonly IFormatterResolver Instance = new AramisDataInfoResolver();

        private AramisDataInfoResolver()
        {
        }

        // GetFormatter<T>'s get cost should be minimized so use type cache.
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter = (IMessagePackFormatter<T>)AramisDataInfoResolverGetFormatterHelper.GetFormatter(typeof(T));
        }
    }

    internal static class AramisDataInfoResolverGetFormatterHelper
    {
        // If type is concrete type, use type-formatter map
        private static readonly Dictionary<Type, object> formatterMap = new ()
        {
            { typeof(AramisDataInfo), new AramisDataInfoFormatter() },
            { typeof(IDataFileInfo), new IDataFileInfoFormatter() },

            // add more your own custom serializers.
        };

        internal static object GetFormatter(Type t)
        {
            if (formatterMap.TryGetValue(t, out object formatter))
            {
                return formatter;
            }

            // If target type is generics, use MakeGenericType.
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                return Activator.CreateInstance(typeof(ValueTupleFormatter<,>).MakeGenericType(t.GenericTypeArguments));
            }

            // If type can not get, must return null for fallback mechanism.
            return null;
        }
    }
}
