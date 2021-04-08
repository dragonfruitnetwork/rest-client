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

        public Encoding Encoding { get; set; }

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
            var serializer = new XmlSerializer(typeof(T));

            return (T)serializer.Deserialize(reader);
        }
    }
}
