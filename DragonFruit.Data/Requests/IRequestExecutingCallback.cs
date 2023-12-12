// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Threading.Tasks;

namespace DragonFruit.Data.Requests
{
    public interface IRequestExecutingCallback
    {
        public void OnRequestExecuting(ApiClient client);
    }

    public interface IAsyncRequestExecutingCallback
    {
        public ValueTask OnRequestExecuting(ApiClient client);
    }
}
