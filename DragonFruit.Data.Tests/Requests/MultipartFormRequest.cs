// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Text;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    [FormBodyType(FormBodyType.Multipart)]
    public partial class MultipartFormRequest : ApiRequest
    {
        public override string RequestPath => "https://postman-echo.com/post";
        public override HttpMethod RequestMethod => HttpMethod.Post;

        [RequestParameter(ParameterType.Query, "c")]
        public string Content => "content";

        [RequestParameter(ParameterType.Form, "bytes")]
        public byte[] ContentBytes => Encoding.UTF8.GetBytes(Content);

        [RequestParameter(ParameterType.Form, "file")]
        public Stream File => new MemoryStream(ContentBytes);
    }
}
