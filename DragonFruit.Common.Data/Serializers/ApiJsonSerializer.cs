// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using DragonFruit.Common.Data.Utils;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Serializers
{
    public class ApiJsonSerializer : ApiSerializer
    {
        private JsonSerializer _serializer;

        public override string ContentType => "application/json";

        /// <summary>
        /// Creates a <see cref="ApiJsonSerializer"/> using the default culture
        /// </summary>
        public ApiJsonSerializer()
        {
        }

        /// <summary>
        /// Creates a <see cref="ApiJsonSerializer"/> using the specified <see cref="CultureInfo"/>
        /// </summary>
        [Obsolete("This will be removed in the future, use Serializer.Configure instead")]
        public ApiJsonSerializer(CultureInfo culture, Encoding encoding = null, bool autoDetectEncoding = true)
            : this(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Culture = culture
            }, encoding, autoDetectEncoding)
        {
        }

        /// <summary>
        /// Creates a <see cref="ApiJsonSerializer"/> using the specified <see cref="JsonSerializerSettings"/>
        /// </summary>
        [Obsolete("This will be removed in the future, use Serializer.Configure instead")]
        public ApiJsonSerializer(JsonSerializerSettings settings, Encoding encoding = null, bool autoDetectEncoding = true)
            : this(JsonSerializer.Create(settings), encoding, autoDetectEncoding)
        {
        }

        /// <summary>
        /// Creates a <see cref="ApiJsonSerializer"/> using the specified <see cref="JsonSerializer"/>
        /// </summary>
        [Obsolete("This will be removed in the future, use Serializer.Configure instead")]
        public ApiJsonSerializer(JsonSerializer serializer, Encoding encoding = null, bool autoDetectEncoding = true)
            : this(encoding, autoDetectEncoding)
        {
            Serializer = serializer;
        }

        /// <summary>
        /// Creates a <see cref="ApiJsonSerializer"/> using the specified <see cref="Encoding"/>
        /// </summary>
        [Obsolete("This will be removed in the future, use Serializer.Configure instead")]
        private ApiJsonSerializer(Encoding encoding, bool autoDetectEncoding)
        {
            Encoding = encoding;
            AutoDetectEncoding = autoDetectEncoding;
        }

        public JsonSerializer Serializer
        {
            get => _serializer ??= new JsonSerializer { Culture = CultureUtils.DefaultCulture, Formatting = Formatting.Indented };
            set => _serializer = value;
        }

        public override HttpContent Serialize<T>(T input) where T : class
        {
            var stream = new MemoryStream();

            // these must dispose before processing the stream, as we need any/all buffers flushed
            using (var streamWriter = new StreamWriter(stream, Encoding, 4096, true))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.ArrayPool = JsonArrayPool.Instance;
                Serializer.Serialize(jsonWriter, input);
            }

            return GetHttpContent(stream);
        }

        public override T Deserialize<T>(Stream input) where T : class
        {
            using var sr = AutoDetectEncoding switch
            {
                true => new StreamReader(input, true),

                false when Encoding is null => new StreamReader(input),
                false => new StreamReader(input, Encoding)
            };

            using var reader = new JsonTextReader(sr)
            {
                ArrayPool = JsonArrayPool.Instance
            };

            return Serializer.Deserialize<T>(reader);
        }
    }

    /// <summary>
    /// A wrapper for the <see cref="T:System.Buffers.ArrayPool`1" /> that implements <see cref="T:Newtonsoft.Json.IArrayPool`1" />
    /// </summary>
    /// <remarks>
    /// Taken from https://github.com/JamesNK/Newtonsoft.Json/blob/52e257ee57899296d81a868b32300f0b3cfeacbe/Src/Newtonsoft.Json.Tests/DemoTests.cs#L709
    /// </remarks>
    internal class JsonArrayPool : IArrayPool<char>
    {
        public static readonly JsonArrayPool Instance = new JsonArrayPool();

        public char[] Rent(int minimumLength) => ArrayPool<char>.Shared.Rent(minimumLength);
        public void Return(char[] array) => ArrayPool<char>.Shared.Return(array);
    }
}
