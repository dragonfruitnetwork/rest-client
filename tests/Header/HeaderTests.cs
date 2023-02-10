// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Threading.Tasks;
using DragonFruit.Data.Extensions;
using DragonFruit.Data.Tests.Requests;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DragonFruit.Data.Tests.Header
{
    [TestFixture]
    public class HeaderTests : ApiTest
    {
        private const string HeaderName = "x-dfn-test";
        private static readonly Random Rng = new();

        /// <summary>
        /// Test whether client headers are sent and changed successfully
        /// </summary>
        [TestCase]
        public async Task HeaderTest()
        {
            var headerValue = Rng.Next().ToString();
            var request = new EchoRequest();

            Client.Headers[HeaderName] = headerValue;
            var response = await Client.PerformAsync<JObject>(request);
            Assert.AreEqual(headerValue, (string)response["headers"][HeaderName]);

            headerValue = Rng.Next().ToString();
            Client.Headers[HeaderName] = headerValue;

            response = await Client.PerformAsync<JObject>(request);
            Assert.AreEqual(headerValue, (string)response["headers"][HeaderName]);
        }

        /// <summary>
        /// Test whether a header sent in a request is recieved successfully
        /// </summary>
        [TestCase]
        public async Task PerRequestHeaderTest()
        {
            var headerValue = Rng.Next().ToString();

            var request = new EchoRequest().WithHeader(HeaderName, headerValue);
            var response = await Client.PerformAsync<JObject>(request);

            Assert.AreEqual((string)response["headers"]![HeaderName], headerValue);
        }
    }
}
