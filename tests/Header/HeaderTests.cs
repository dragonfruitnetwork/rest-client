// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;
using DragonFruit.Data.Extensions;
using DragonFruit.Data.Tests.Header.Objects;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

#endregion

#pragma warning disable 1998

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
        public void HeaderTest()
        {
            var headerValue = Rng.Next().ToString();
            var request = new EchoRequest();

            Client.Headers[HeaderName] = headerValue;
            var response = Client.Perform<JObject>(request);
            Assert.AreEqual(headerValue, (string)response["headers"][HeaderName]);

            headerValue = Rng.Next().ToString();
            Client.Headers[HeaderName] = headerValue;
            response = Client.Perform<JObject>(request);
            Assert.AreEqual(headerValue, (string)response["headers"][HeaderName]);
        }

        /// <summary>
        /// Test whether a header sent in a request is recieved successfully
        /// </summary>
        [TestCase]
        public void PerRequestHeaderTest()
        {
            var headerValue = Rng.Next().ToString();

            var request = new EchoRequest().WithHeader(HeaderName, headerValue);
            var response = Client.Perform<JObject>(request);

            Assert.AreEqual((string)response["headers"]![HeaderName], headerValue);
        }
    }
}
