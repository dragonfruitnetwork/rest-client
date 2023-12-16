// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class CustomHttpContentRequest : ApiRequest
    {
        public override string RequestPath => "https://postman-echo.com/patch";
        public override HttpMethod RequestMethod => HttpMethod.Patch;

        [RequestBody]
        public HttpContent RequestContent => new StringContent("Test Content");
    }
}
