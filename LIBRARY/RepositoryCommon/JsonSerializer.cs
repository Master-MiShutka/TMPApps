namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public static class JsonSerializer
    {
        /// <summary>
        /// Размер данных
        /// </summary>
        private static long dataSize;

        /// <summary>
        /// Размер упакованных данных
        /// </summary>
        private static long compressedDataSize;

        public static long CompressedDataSize { get; private set; }

        public static async Task<(long, long)> GzJsonSerializeAsync(object model, string fileName)
        {
            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            using System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress);
            using MemoryStream memoryStream = new MemoryStream();
            await JsonSerializeObjectAsync(model, memoryStream);
            await JsonSerializeObjectAsync(model, gz);
            await gz.FlushAsync();
            dataSize = memoryStream.Length;
            compressedDataSize = gz.Length;
            return (dataSize, compressedDataSize);
        }

        public static async Task<byte[]> JsonSerializeToBytesAsync<T>(T model)
        {
            // byte[] bytes = MessagePack.MessagePackSerializer.Serialize(model, lz4Options);
            // byte[] bytes = Utf8Json.JsonSerializer.Serialize(model);
            using MemoryStream stream = new MemoryStream();

            var optinos = MessagePack.MessagePackSerializer.DefaultOptions.WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray);

            await MessagePack.MessagePackSerializer.SerializeAsync<T>(stream, model, optinos);

            // await JsonSerializeObjectAsync(model, stream);
            using StreamReader reader = new StreamReader(stream);
            byte[] result = stream.ToArray();
            return result;
        }

        public static async Task<string> JsonSerializeToStringAsync(object model)
        {
            using MemoryStream stream = new MemoryStream();
            await JsonSerializeObjectAsync(model, stream);
            using StreamReader reader = new StreamReader(stream);
            string result = await reader.ReadToEndAsync();
            return result;
        }

        public static async Task<bool> JsonSerializeToStreamAsync(object model, Stream stream)
        {
            stream = new MemoryStream();
            await JsonSerializeObjectAsync(model, stream);
            dataSize = stream.Position;
            return true;
        }

        public static async Task<bool> JsonSerializeAsync(object model, string fileName)
        {
            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            {
                await JsonSerializeObjectAsync(model, fs);
                dataSize = fs.Position;
            }

            return true;
        }

        private static async Task JsonSerializeObjectAsync(object model, Stream utf8Stream)
        {
            await System.Text.Json.JsonSerializer.SerializeAsync(utf8Stream, model);
        }
    }
}
