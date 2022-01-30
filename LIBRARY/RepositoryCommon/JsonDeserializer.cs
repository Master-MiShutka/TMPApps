namespace TMP.Common.RepositoryCommon
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public static class JsonDeserializer
    {
        public static async Task<T> FromFileAsync<T>(string fileName)
        {
            if (System.IO.File.Exists(fileName) == false)
            {
                return default;
            }

            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            using StreamReader reader = new StreamReader(fs, Encoding.UTF8);
            T result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(fs);
            return result;
        }

        public static async Task<T> FromStreamAsync<T>(Stream stream)
        {
            if (stream == null)
            {
                return default;
            }

            T result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream);
            return result;
        }

        public static T FromBytes<T>(byte[] data)
        {
            if (data == null)
            {
                return default;
            }

            T result = System.Text.Json.JsonSerializer.Deserialize<T>(data);
            return result;
        }

        public static async Task<T> FromGzFileAsync<T>(string fileName)
        {
            if (System.IO.File.Exists(fileName) == false)
            {
                return default;
            }

            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            using System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false);
            T result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(gz);
            return result;
        }
    }
}
