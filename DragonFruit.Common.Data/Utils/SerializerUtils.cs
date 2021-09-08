// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using DragonFruit.Common.Data.Serializers;

namespace DragonFruit.Common.Data.Utils
{
    public static class SerializerUtils
    {
        [Obsolete("Now available as ApiSerializer.GetHttpContent(stream). This will be removed in the future")]
        public static HttpContent ProcessStream(ISerializer serializer, Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamContent(stream);

            content.Headers.ContentLength = stream.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(serializer.ContentType)
            {
                CharSet = serializer.Encoding.HeaderName
            };

            return content;
        }
    }
}
