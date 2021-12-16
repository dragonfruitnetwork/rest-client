// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System.IO;
using System.Threading.Tasks;

#endregion

namespace DragonFruit.Data.Serializers
{
    /// <summary>
    /// Extends a <see cref="ApiSerializer"/> by providing async support
    /// </summary>
    public interface IAsyncSerializer
    {
        Task<T> DeserializeAsync<T>(Stream input) where T : class;
    }
}
