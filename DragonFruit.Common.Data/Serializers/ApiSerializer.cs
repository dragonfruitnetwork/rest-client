﻿// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Text;

#pragma warning disable 618

namespace DragonFruit.Common.Data.Serializers
{
    /// <summary>
    /// Represents the base of a serializer used by the <see cref="ApiClient"/> and <see cref="ApiRequest"/> classes
    /// </summary>
    public abstract class ApiSerializer : ISerializer
    {
        private Encoding _encoding;

        /// <summary>
        /// The content-type header value
        /// </summary>
        public abstract string ContentType { get; }

        /// <summary>
        /// Gets or sets the encoding the <see cref="ApiSerializer"/> uses
        /// </summary>
        public Encoding Encoding
        {
            get => _encoding ?? Encoding.UTF8;
            set => _encoding = value;
        }

        /// <summary>
        /// Whether deserialization should attempt to automatically detect the encoding used
        /// </summary>
        public bool AutoDetectEncoding { get; set; } = true;

        public abstract HttpContent Serialize<T>(T input) where T : class;
        public abstract T Deserialize<T>(Stream input) where T : class;
    }
}
