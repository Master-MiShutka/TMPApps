namespace TMP.Common.RepositoryCommon
{
    using System.IO;
    using System.Threading.Tasks;

    public static class MessagePackSerializer
    {
        /// <summary>
        /// Размер данных
        /// </summary>
        public static long DataSize { get; private set; }

        public static MessagePack.MessagePackSerializerOptions Options { get; set; } = MessagePack.MessagePackSerializer.DefaultOptions
                .WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray);

        public static async Task<MemoryStream> ToStreamAsync<T>(T model)
        {
            MemoryStream stream = new MemoryStream();
            await MessagePack.MessagePackSerializer.SerializeAsync<T>(stream, model, Options);
            DataSize = stream.Position;
            return stream;
        }

        public static async Task<byte[]> ToBytesAsync<T>(T model)
        {
            using MemoryStream stream = await ToStreamAsync(model);
            using StreamReader reader = new StreamReader(stream);
            byte[] result = stream.ToArray();
            return result;
        }

        public static byte[] ToBytes<T>(T model)
        {
            using MemoryStream stream = ToStreamAsync(model).Result;
            using StreamReader reader = new StreamReader(stream);
            byte[] result = stream.ToArray();
            return result;
        }

        public static async Task<string> ToStringAsync<T>(T model)
        {
            using MemoryStream stream = await ToStreamAsync(model);

            using StreamReader reader = new StreamReader(stream);
            string result = await reader.ReadToEndAsync();
            return result;
        }

        public static async Task<bool> ToFileAsync<T>(T model, string fileName)
        {
            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            {
                await MessagePack.MessagePackSerializer.SerializeAsync<T>(fs, model, Options);
                DataSize = fs.Position;
            }

            return true;
        }
    }
}
