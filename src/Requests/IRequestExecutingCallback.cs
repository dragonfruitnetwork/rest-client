// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Data.Requests
{
    /// <summary>
    /// Specifies the <see cref="ApiRequest"/> should have its <see cref="OnRequestExecuting"/> method called after when the request is being executed
    /// </summary>
    public interface IRequestExecutingCallback
    {
        /// <summary>
        /// Overridable method for specifying an action to occur before sending the request to the <see cref="HttpClient"/>
        /// </summary>
        void OnRequestExecuting(ApiClient client);
    }
}
