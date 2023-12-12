// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using DragonFruit.Data.Serializers;

namespace DragonFruit.Data.Requests
{
    public interface IRequestBuilder
    {
        HttpRequestMessage BuildRequest(SerializerResolver serializerResolver);
    }
}
