// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    [FormBodyType(FormBodyType.Multipart)]
    public partial class InheritedMultipartEchoRequest : InheritedEchoRequest
    {
        // no additional properties, just changed body type
    }
}
