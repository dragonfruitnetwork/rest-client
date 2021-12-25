// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DragonFruit.Data.Serializers.SystemJson
{
    public class ApiSystemTextJsonSerializer : ApiSerializer, IAsyncSerializer
    {
        private JsonSerializerOptions _serializerOptions;

        public override string ContentType => "application/json";

        public override Encoding Encoding
        {
            get => base.Encoding;
            set => throw new NotSupportedException("System.Text.Json is UTF-8 only");
        }

        public JsonSerializerOptions SerializerOptions
        {
            get => _serializerOptions ??= new JsonSerializerOptions();
            set => _serializerOptions = value;
        }

        public override HttpContent Serialize<T>(T input)
        {
            var stream = GetStream(false);
            JsonSerializer.Serialize(stream, input, SerializerOptions);

            return GetHttpContent(stream);
        }

        public override T Deserialize<T>(Stream input) => JsonSerializer.Deserialize<T>(input, SerializerOptions);

        public Task<T> DeserializeAsync<T>(Stream input) where T : class => JsonSerializer.DeserializeAsync<T>(input, SerializerOptions).AsTask();

        /// <summary>
        /// Registers <see cref="JsonDocument"/> to always use the <see cref="ApiSystemTextJsonSerializer"/>
        /// </summary>
        public static void RegisterDefaults() => SerializerResolver.Register<JsonDocument, ApiSystemTextJsonSerializer>();
    }
}
