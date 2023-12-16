// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Globalization;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Data.Serializers.Newtonsoft
{
    public class NewtonsoftJsonSerializer : ApiSerializer
    {
        private JsonSerializer _serializer;

        public override string ContentType => "application/json";

        public JsonSerializer Serializer
        {
            get => _serializer ??= new JsonSerializer { Culture = CultureInfo.InvariantCulture };
            set => _serializer = value;
        }

        public override HttpContent Serialize<T>(T input)
        {
            var stream = new MemoryStream();

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
            using var streamReader = new StreamReader(input, Encoding, true, 4096);
            using var reader = new JsonTextReader(streamReader);

            reader.ArrayPool = JsonArrayPool.Instance;
            return Serializer.Deserialize<T>(reader);
        }

        /// <summary>
        /// Registers Newtonsoft.Json Linq objects to be resolved by this serializer
        /// </summary>
        public static void RegisterDefaults()
        {
            SerializerResolver.Register<JArray, ApiJsonSerializer>();
            SerializerResolver.Register<JToken, ApiJsonSerializer>();
            SerializerResolver.Register<JObject, ApiJsonSerializer>();
        }
    }
}
