// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DragonFruit.Common.Data.Serializers
{
    public class ApiXmlSerializer : ISerializer
    {
        public string ContentType => "application/xml";

        public StringContent Serialize<T>(T input) where T : class
        {
            using (StringWriter textWriter = new StringWriter())
            {
                new XmlSerializer(typeof(T)).Serialize(textWriter, input);
                return new StringContent(textWriter.ToString(), textWriter.Encoding, ContentType);
            }
        }

        public T Deserialize<T>(Task<Stream> input) where T : class
        {
            var serialiser = new XmlSerializer(typeof(T));

            using (Stream stream = input.Result)
            using (StreamReader sr = new StreamReader(stream))
            using (StringReader stringReader = new StringReader(sr.ReadToEndAsync().Result))
            {
                return (T)serialiser.Deserialize(stringReader);
            }
        }
    }
}
