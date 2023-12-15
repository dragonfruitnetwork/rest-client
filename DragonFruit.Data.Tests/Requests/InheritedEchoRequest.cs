// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class InheritedEchoRequest : BasicEchoRequest
    {
        public override string RequestPath => "https://postman-echo.com/post";
        public override HttpMethod RequestMethod => HttpMethod.Post;

        [RequestParameter(ParameterType.Form, "extra")]
        public string Additional => "additional_content";
    }
}
