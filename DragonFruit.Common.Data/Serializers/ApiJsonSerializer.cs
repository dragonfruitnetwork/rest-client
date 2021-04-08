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

        public string ContentType => "application/json";

        public Encoding Encoding { get; set; }
        public JsonSerializer Serializer { get; set; }

        public HttpContent Serialize<T>(T input) where T : class
        {
            Encoding encoding;
            var stream = new MemoryStream();

            // these must dispose before processing the stream, as we need any/all buffers flushed
            using (var streamWriter = new StreamWriter(stream, Encoding, -1, true))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                encoding = streamWriter.Encoding;
                Serializer.Serialize(jsonWriter, input);
            }

            return SerializerUtils.ProcessStream(this, stream, encoding);
        }

        public T Deserialize<T>(Stream input) where T : class
        {
            using var sr = new StreamReader(input);
            using var reader = new JsonTextReader(sr);

            return Serializer.Deserialize<T>(reader);
        }
    }
}
