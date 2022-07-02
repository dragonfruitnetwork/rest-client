// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.Data.Serializers
{
    /// <summary>
    /// Extends a <see cref="ApiSerializer"/> by providing async support
    /// </summary>
    public interface IAsyncSerializer
    {
        ValueTask<T> DeserializeAsync<T>(Stream input) where T : class;
    }
}
