// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.Data.Serializers
{
    public interface IAsyncSerializer
    {
        /// <summary>
        /// Asynchronously deserialize the provided <see cref="Stream"/> into an object of the specified type
        /// </summary>
        ValueTask<T> DeserializeAsync<T>(Stream input) where T : class;
    }
}
