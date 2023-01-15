// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DragonFruit.Data.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Data.Serializers.Newtonsoft
{
    public class ApiJsonSerializer : ApiSerializer, IAsyncSerializer
    {
        private JsonSerializer _serializer;

        public override string ContentType => "application/json";

        public JsonSerializer Serializer
        {
            get => _serializer ??= new JsonSerializer { Culture = CultureUtils.DefaultCulture, Formatting = Formatting.Indented };
            set => _serializer = value;
        }

        public override HttpContent Serialize(object input)
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
            var reader = GetReader(input);
            return Serializer.Deserialize<T>(reader);
        }

        public async ValueTask<T> DeserializeAsync<T>(Stream input) where T : class
        {
            var reader = GetReader(input);
            var token = await JToken.LoadAsync(reader).ConfigureAwait(false);

            return token.ToObject<T>();
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

        private JsonTextReader GetReader(Stream input)
        {
            var sr = AutoDetectEncoding switch
            {
                true => new StreamReader(input, true),

                false when Encoding is null => new StreamReader(input),
                false => new StreamReader(input, Encoding)
            };

            return new JsonTextReader(sr)
            {
                ArrayPool = JsonArrayPool.Instance
            };
        }
    }
}
