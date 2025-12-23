// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class SpecialTypeRequest : ApiRequest
    {
        public override string RequestPath => "https://postman-echo.com/get";

        [RequestParameter(ParameterType.Query, "users")]
        [EnumerableOptions(EnumerableOption.Concatenated, ":")]
        public IReadOnlyCollection<string> Usernames => ["test", "test_1a"];

        [RequestParameter(ParameterType.Query, "ids")]
        [EnumerableOptions(EnumerableOption.Concatenated)]
        public IEnumerable<int> UserIds => [1, 2];
    }
}
