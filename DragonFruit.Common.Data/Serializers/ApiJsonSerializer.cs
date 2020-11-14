// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using DragonFruit.Common.Data.Utils;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Serializers
{
    public class ApiJsonSerializer : ISerializer
    {
        public ApiJsonSerializer()
            : this(CultureUtils.DefaultCulture)
        {
        }

        public ApiJsonSerializer(JsonSerializerSettings settings)
            : this(JsonSerializer.Create(settings))
        {
        }

        public ApiJsonSerializer(CultureInfo culture)
            : this(JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Culture = culture
            }))
        {
        }

        public ApiJsonSerializer(JsonSerializer serializer)
        {
            Serializer = serializer;
        }

        public JsonSerializer Serializer { get; set; }

        public string ContentType => "application/json";

        public StringContent Serialize<T>(T input) where T : class
        {
            var builder = new StringBuilder();
            using var writer = new StringWriter(builder);
            using var jsonWriter = new JsonTextWriter(writer);

            Serializer.Serialize(jsonWriter, input);
            return new StringContent(builder.ToString(), writer.Encoding, ContentType);
        }

        public T Deserialize<T>(Stream input) where T : class
        {
            using var sr = new StreamReader(input);
            using JsonReader reader = new JsonTextReader(sr);

            return Serializer.Deserialize<T>(reader);
        }
    }
}
