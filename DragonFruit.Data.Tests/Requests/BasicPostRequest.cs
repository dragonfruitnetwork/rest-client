// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class BasicPostRequest : ApiRequest
    {
        public override HttpMethod RequestMethod => HttpMethod.Post;
        public override string RequestPath => "https://postman-echo.com/post";

        [RequestParameter(ParameterType.Query, "q1")]
        public string Query1 => "test_query_1";

        [RequestParameter(ParameterType.Query, "q2")]
        public string Query2 => "test_query_2";
    }
}
