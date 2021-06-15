// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using DragonFruit.Common.Data.Utils;

namespace DragonFruit.Common.Data.Serializers
{
    public class ApiXmlSerializer : ISerializer
    {
        public string ContentType => "application/xml";

        public ApiXmlSerializer(Encoding encoding = null, bool autoDetectEncoding = true)
        {
            Encoding = encoding;
            AutoDetectEncoding = autoDetectEncoding;
        }

        public Encoding Encoding { get; }
        public bool AutoDetectEncoding { get; }

        public HttpContent Serialize<T>(T input) where T : class
        {
            Encoding encoding;
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding, -1, true))
            {
                encoding = writer.Encoding;
                new XmlSerializer(typeof(T)).Serialize(writer, input);
            }

            return SerializerUtils.ProcessStream(this, stream, encoding);
        }

        public T Deserialize<T>(Stream input) where T : class
        {
            using var reader = new StreamReader(input);
            using TextReader reader = AutoDetectEncoding switch
            {
                true => new StreamReader(input, true),

                false when Encoding is null => new StreamReader(input),
                false => new StreamReader(input, Encoding)
            };

            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(reader);
        }
    }
}
