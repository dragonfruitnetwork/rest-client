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

        public override HttpContent Serialize<T>(T input)
        {
            ValidateType<T>();

            // html is usually larger than 80kb
            var stream = GetStream(true);
            (input as HtmlDocument)!.Save(stream, Encoding);

            return GetHttpContent(stream);
        }

        public override T Deserialize<T>(Stream input)
        {
            ValidateType<T>();

            var document = new HtmlDocument();
            document.Load(input, Encoding, AutoDetectEncoding);

            return document as T; // where T is validated as a HtmlDocument
        }

        private static void ValidateType<T>()
        {
            if (typeof(T) != typeof(HtmlDocument))
            {
                throw new ArgumentException($"Cannot process {typeof(T).Name}", nameof(T));
            }
        }

        /// <summary>
        /// Registers the <see cref="ApiHtmlSerializer"/> to resolve <see cref="HtmlDocument"/> objects
        /// </summary>
        public static void RegisterDefaults() => SerializerResolver.Register<HtmlDocument, ApiHtmlSerializer>();
    }
}
