// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestEntities
{
    public partial class BasicHttpRequest : ApiRequest
    {
        public override string RequestPath => "https://postman-echo.com/get";

        [RequestParameter(ParameterType.Query, "q1")]
        public string Query1 { get; set; }

        [RequestParameter(ParameterType.Query, "q2")]
        public string Query2 { get; set; }

        [RequestParameter(ParameterType.Query, "q3")]
        public string Query3 { get; set; }
    }
}
