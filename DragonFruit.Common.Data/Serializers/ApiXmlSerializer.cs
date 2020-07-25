using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DragonFruit.Common.Data.Helpers;

namespace DragonFruit.Common.Data.Serializers
{
    public class ApiXmlSerializer : ISerializer
    {
        private readonly IDictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>();

        public string ContentType => "application/xml";

        public StringContent Serialize<T>(T input) where T : class
        {
            var type = typeof(T);
            var serializer = _serializers.GetOrSet(type, () => new XmlSerializer(type));

            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, input);
                return new StringContent(textWriter.ToString(), Encoding.UTF8, "application/xml");
            }
        }

        public T Deserialize<T>(Task<Stream> input) where T : class
        {
            var type = typeof(T);
            var serializer = _serializers.GetOrSet(type, () => new XmlSerializer(type));

            using (StreamReader sr = new StreamReader(input.Result))
            using (StringReader stringReader = new StringReader(sr.ReadToEndAsync().Result))
            {
                return (T)serializer.Deserialize(stringReader);
            }
        }
    }
}
