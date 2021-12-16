// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System.IO;
using System.Net.Http;
using System.Xml.Serialization;

#endregion

namespace DragonFruit.Data.Serializers
{
    public class ApiXmlSerializer : ApiSerializer
    {
        public override string ContentType => "application/xml";

        public override HttpContent Serialize<T>(T input) where T : class
        {
            var stream = GetStream(false);

            using (var writer = new StreamWriter(stream, Encoding, 4096, true))
            {
                new XmlSerializer(typeof(T)).Serialize(writer, input);
            }

            return GetHttpContent(stream);
        }

        public override T Deserialize<T>(Stream input) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            using TextReader reader = AutoDetectEncoding switch
            {
                true => new StreamReader(input, true),

                false when Encoding is null => new StreamReader(input),
                false => new StreamReader(input, Encoding)
            };

            return (T)serializer.Deserialize(reader);
        }
    }
}
