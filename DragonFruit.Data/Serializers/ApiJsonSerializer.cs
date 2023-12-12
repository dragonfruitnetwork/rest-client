// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DragonFruit.Data.Serializers
{
    public class ApiJsonSerializer : ApiSerializer, IAsyncSerializer
    {
        public override string ContentType => "application/json";

        public override Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Gets or sets the current <see cref="JsonSerializerOptions"/> used.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; set; } = JsonSerializerOptions.Default;

        public override HttpContent Serialize<T>(T input)
        {
            var utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(input, typeof(T), SerializerOptions);
            var httpContent = new ByteArrayContent(utf8Bytes);

            httpContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType)
            {
                CharSet = Encoding.HeaderName
            };

            return httpContent;
        }

        public override T Deserialize<T>(Stream input)
        {
            return JsonSerializer.Deserialize<T>(input, SerializerOptions);
        }

        public ValueTask<T> DeserializeAsync<T>(Stream input) where T : class
        {
            return JsonSerializer.DeserializeAsync<T>(input, SerializerOptions);
        }
    }
}
