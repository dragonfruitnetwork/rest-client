// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;

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

        public StringContent Serialize<T>(T input) where T : class
        {
            using StringWriter textWriter = new StringWriter();

            new XmlSerializer(typeof(T)).Serialize(textWriter, input);
            return new StringContent(textWriter.ToString(), textWriter.Encoding, ContentType);
        }

        public T Deserialize<T>(Stream input) where T : class
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
