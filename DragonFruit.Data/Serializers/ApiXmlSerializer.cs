// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Serialization;

namespace DragonFruit.Data.Serializers
{
    public class ApiXmlSerializer : ApiSerializer
    {
        public override string ContentType => "application/xml";

        public override HttpContent Serialize<T>(T input)
        {
            var stream = new MemoryStream(4096);

            using (var writer = new StreamWriter(stream, Encoding, 4096, true))
            {
                new XmlSerializer(input.GetType()).Serialize(writer, input);
            }

            var xmlContent = new ByteArrayContent(stream.GetBuffer(), 0, (int)stream.Length);

            xmlContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType)
            {
                CharSet = Encoding.HeaderName
            };

            return xmlContent;
        }

        public override T Deserialize<T>(Stream input) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = XmlReader.Create(input);

            if (serializer.CanDeserialize(reader))
            {
                return (T)serializer.Deserialize(reader);
            }

            throw new InvalidOperationException($"Unable to deserialize stream to type {typeof(T).Name}");
        }
    }
}
