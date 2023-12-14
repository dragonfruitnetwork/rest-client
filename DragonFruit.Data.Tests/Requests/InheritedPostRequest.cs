// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class InheritedPostRequest : BasicPostRequest
    {
        [RequestParameter(ParameterType.Query, "extra")]
        public string Additional => "additional_content";
    }
}
