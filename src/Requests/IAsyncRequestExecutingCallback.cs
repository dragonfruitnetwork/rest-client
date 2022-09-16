// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using System.Threading.Tasks;

namespace DragonFruit.Data.Requests
{
    /// <summary>
    /// Specifies the <see cref="ApiRequest"/> should have its <see cref="OnRequestExecutingAsync"/> method called after when the request is being executed
    /// </summary>
    public interface IAsyncRequestExecutingCallback
    {
        /// <summary>
        /// Overridable method for specifying an action to occur before sending the request to the <see cref="HttpClient"/>.
        /// Unlike <see cref="IRequestExecutingCallback"/>, this will be run asynchronously and must return a <see cref="ValueTask"/>.
        /// </summary>
        ValueTask OnRequestExecutingAsync(ApiClient client);
    }
}
