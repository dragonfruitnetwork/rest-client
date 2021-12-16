// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Data.Tests.Requests
{
    public class EchoRequest : ApiRequest
    {
        public override string Path => $"https://postman-echo.com/{Method.ToString().ToLowerInvariant()}";
        protected override Methods Method { get; }

        public EchoRequest(Methods method = Methods.Get)
        {
            Method = method;
        }
    }
}
