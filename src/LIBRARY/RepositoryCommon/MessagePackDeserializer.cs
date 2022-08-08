namespace TMP.Common.RepositoryCommon
{
    using System.IO;
    using System.Threading.Tasks;

    public static class MessagePackDeserializer
    {
        public static MessagePack.MessagePackSerializerOptions Options { get; set; } = MessagePack.MessagePackSerializer.DefaultOptions
                .WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray);

        public static async Task<T> FromFileAsync<T>(string fileName)
        {
            if (System.IO.File.Exists(fileName) == false)
            {
                return default;
            }

            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            T result = await MessagePack.MessagePackSerializer.DeserializeAsync<T>(fs, Options);
            return result;
        }

        public static async Task<T> FromStreamAsync<T>(Stream stream)
        {
            if (stream == null)
            {
                return default;
            }

            T result;
            result = await MessagePack.MessagePackSerializer.DeserializeAsync<T>(stream, Options);
            return result;
        }

        public static T FromBytes<T>(byte[] data)
        {
            if (data == null)
            {
                return default;
            }

            T result = MessagePack.MessagePackSerializer.Deserialize<T>(data, Options);
            return result;
        }
    }
}
