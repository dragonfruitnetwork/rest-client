// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DragonFruit.Common.Data.Serializers;

namespace DragonFruit.Common.Data.Utils
{
    public static class SerializerUtils
    {
        public static HttpContent ProcessStream(ISerializer serializer, Stream stream, Encoding encoding)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamContent(stream);

            content.Headers.ContentLength = stream.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(serializer.ContentType);

            return content;
        }
    }
}
