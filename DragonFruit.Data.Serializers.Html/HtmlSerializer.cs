// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;

namespace DragonFruit.Data.Serializers.Html
{
    public class HtmlSerializer : ApiSerializer
    {
        public override string ContentType => "text/html";

        public override bool IsGeneric => false;

        public override HttpContent Serialize<T>(T input)
        {
            if (input is not HtmlDocument document)
            {
                throw new ArgumentException($"Cannot process {input.GetType().Name}", nameof(input));
            }

            var stream = new MemoryStream();
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
            document.Load(input, Encoding);

            return document as T; // T is already validated as a HtmlDocument
        }

        /// <summary>
        /// Registers the <see cref="HtmlSerializer"/> to resolve <see cref="HtmlDocument"/> objects
        /// </summary>
        public static void RegisterDefaults() => SerializerResolver.Register<HtmlDocument, HtmlSerializer>();
    }
}
