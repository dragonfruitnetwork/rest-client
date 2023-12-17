// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DragonFruit.Data.Serializers
{
    internal class InternalStreamSerializer : ApiSerializer, IAsyncSerializer
    {
        public override string ContentType => "application/octet-stream";

        public override HttpContent Serialize<T>(T input)
        {
            throw new NotSupportedException("Stream serialization is currently one-way");
        }

        public override T Deserialize<T>(Stream input)
        {
            var stream = GetStream<T>();
            input.CopyTo(stream);

            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            return stream as T;
        }

        public async ValueTask<T> DeserializeAsync<T>(Stream input) where T : class
        {
            var stream = GetStream<T>();
            await input.CopyToAsync(stream).ConfigureAwait(false);

            await stream.FlushAsync().ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);

            return stream as T;
        }

        private Stream GetStream<T>()
        {
            if (typeof(T) == typeof(MemoryStream))
            {
                return new MemoryStream();
            }

            return File.Create(Path.GetTempFileName(), 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);
        }
    }
}
