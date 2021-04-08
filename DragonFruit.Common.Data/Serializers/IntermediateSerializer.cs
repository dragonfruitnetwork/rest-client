// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Text;

namespace DragonFruit.Common.Data.Serializers
{
    /// <summary>
    /// An <see cref="ISerializer"/> that acts as a proxying layer through a serializer that exposes methods to override/alter.
    /// </summary>
    public abstract class IntermediateSerializer : ISerializer
    {
        private readonly ISerializer _baseSerializer;

        protected IntermediateSerializer(ISerializer baseSerializer)
        {
            _baseSerializer = baseSerializer;
        }

        public string ContentType => _baseSerializer.ContentType;

        public Encoding Encoding
        {
            get => _baseSerializer.Encoding;
            set => _baseSerializer.Encoding = value;
        }

        public virtual HttpContent Serialize<T>(T input) where T : class => _baseSerializer.Serialize(input);
        public virtual T Deserialize<T>(Stream input) where T : class => _baseSerializer.Deserialize<T>(input);
    }
}
