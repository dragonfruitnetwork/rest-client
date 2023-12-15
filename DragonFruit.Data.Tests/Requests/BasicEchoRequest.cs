// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class BasicEchoRequest : ApiRequest
    {
        public override string RequestPath => "https://postman-echo.com/get";

        [RequestParameter(ParameterType.Query, "q1")]
        public string Query1 => "test_query_1";

        [RequestParameter(ParameterType.Query, "q2")]
        public string Query2 => "test_query_2";

        [RequestParameter(ParameterType.Query, "q3")]
        public static string StaticQuery3 => "test_query_3";
    }
}
