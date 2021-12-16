// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System.IO;
using System.Net.Http;
using DragonFruit.Data.Utils;
using Newtonsoft.Json;

#endregion

namespace DragonFruit.Data.Serializers.Newtonsoft
{
    public class ApiJsonSerializer : ApiSerializer
    {
        private JsonSerializer _serializer;

        public override string ContentType => "application/json";

        public JsonSerializer Serializer
        {
            get => _serializer ??= new JsonSerializer { Culture = CultureUtils.DefaultCulture, Formatting = Formatting.Indented };
            set => _serializer = value;
        }

        public override HttpContent Serialize<T>(T input) where T : class
        {
            var stream = GetStream(false);

            // these must dispose before processing the stream, as we need any/all buffers flushed
            using (var streamWriter = new StreamWriter(stream, Encoding, 4096, true))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.ArrayPool = JsonArrayPool.Instance;
                Serializer.Serialize(jsonWriter, input);
            }

            return GetHttpContent(stream);
        }

        public override T Deserialize<T>(Stream input) where T : class
        {
            using var sr = AutoDetectEncoding switch
            {
                true => new StreamReader(input, true),

                false when Encoding is null => new StreamReader(input),
                false => new StreamReader(input, Encoding)
            };

            using var reader = new JsonTextReader(sr)
            {
                ArrayPool = JsonArrayPool.Instance
            };

            return Serializer.Deserialize<T>(reader);
        }
    }
}
