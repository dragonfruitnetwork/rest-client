// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Serializers
{
    public class ApiJsonSerializer : ISerializer
    {
        public ApiJsonSerializer()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public ApiJsonSerializer(CultureInfo culture)
        {
            Serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Culture = culture
            });
        }

        public JsonSerializer Serializer { get; set; }

        public string ContentType => "application/json";

        public StringContent Serialize<T>(T input) where T : class
        {
            var builder = new StringBuilder();

            using (var writer = new StringWriter(builder))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                Serializer.Serialize(jsonWriter, input);

                return new StringContent(builder.ToString(), writer.Encoding, ContentType);
            }
        }

        public T Deserialize<T>(Task<Stream> input) where T : class
        {
            using (var stream = input.Result)
            using (var sr = new StreamReader(stream))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return Serializer.Deserialize<T>(reader);
            }
        }
    }
}
