// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DragonFruit.Data
{
    public abstract class ApiSerializer
    {
        private Encoding _encoding;

        /// <summary>
        /// The Content-Type/Accept header value
        /// </summary>
        public abstract string ContentType { get; }

        /// <summary>
        /// Whether this <see cref="ApiSerializer"/> is generic (meaning any class can be serialized to/from).
        /// </summary>
        /// <remarks>
        /// Setting this to <c>false</c> will throw an exception if the serializer is set as a default in a client.
        /// </remarks>
        public virtual bool IsGeneric => true;

        /// <summary>
        /// Gets or sets the encoding the <see cref="ApiSerializer"/> uses
        /// </summary>
        public virtual Encoding Encoding
        {
            get => _encoding ??= new UTF8Encoding(false);
            set => _encoding = value;
        }

        /// <summary>
        /// Serializes the provided input to a <see cref="HttpContent"/> object
        /// </summary>
        public abstract HttpContent Serialize<T>(T input) where T : class;

        /// <summary>
        /// Deserializes the provided <see cref="Stream"/> to the specified type
        /// </summary>
        public abstract T Deserialize<T>(Stream input) where T : class;

        /// <summary>
        /// Converts a <see cref="Stream"/> serialized in the <see cref="ApiSerializer"/> to the <see cref="HttpContent"/> equivalent
        /// </summary>
        /// <param name="stream">The stream to convert</param>
        protected HttpContent GetHttpContent(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamContent(stream);

            content.Headers.ContentLength = stream.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentType)
            {
                CharSet = Encoding.HeaderName
            };

            return content;
        }
    }
}
