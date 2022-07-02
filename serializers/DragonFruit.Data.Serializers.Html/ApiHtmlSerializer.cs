using System;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;

namespace DragonFruit.Data.Serializers.Html
{
    public class ApiHtmlSerializer : ApiSerializer
    {
        public override string ContentType => "text/html";
        public override bool IsGeneric => false;

        public override HttpContent Serialize(object input)
        {
            if (!(input is HtmlDocument document))
            {
                throw new ArgumentException($"Cannot process {input.GetType().Name}", nameof(input));
            }

            // html is usually larger than 80kb
            var stream = GetStream(true);
            document.Save(stream, Encoding);

            return GetHttpContent(stream);
        }

        public override T Deserialize<T>(Stream input)
        {
            if (typeof(T) != typeof(HtmlDocument))
            {
                throw new ArgumentException($"Cannot process {typeof(T).Name}", nameof(T));
            }

            var document = new HtmlDocument();
            document.Load(input, Encoding, AutoDetectEncoding);

            return document as T; // where T is validated as a HtmlDocument
        }

        /// <summary>
        /// Registers the <see cref="ApiHtmlSerializer"/> to resolve <see cref="HtmlDocument"/> objects
        /// </summary>
        public static void RegisterDefaults() => SerializerResolver.Register<HtmlDocument, ApiHtmlSerializer>();
    }
}
