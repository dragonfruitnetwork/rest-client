// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using DragonFruit.Common.Data.Extensions;
using DragonFruit.Common.Data.Tests.Header.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

#pragma warning disable 1998

namespace DragonFruit.Common.Data.Tests.Header
{
    [TestClass]
    public class HeaderTests : ApiTest
    {
        private const string HeaderName = "x-dfn-test";
        private static readonly Random Rng = new Random();

        /// <summary>
        /// Test whether client headers are sent and changed successfully
        /// </summary>
        [TestMethod]
        public void HeaderTest()
        {
            var headerValue = Rng.Next().ToString();
            var request = new EchoRequest();

            Client.Headers[HeaderName] = headerValue;
            var response = Client.Perform<JObject>(request);
            Assert.AreEqual(headerValue, response["headers"]![HeaderName]);

            headerValue = Rng.Next().ToString();
            Client.Headers[HeaderName] = headerValue;
            response = Client.Perform<JObject>(request);
            Assert.AreEqual(headerValue, response["headers"]![HeaderName]);
        }

        /// <summary>
        /// Test whether a header sent in a request is recieved successfully
        /// </summary>
        [TestMethod]
        public void PerRequestHeaderTest()
        {
            var headerValue = Rng.Next().ToString();

            var request = new EchoRequest().WithHeader(HeaderName, headerValue);
            var response = Client.Perform<JObject>(request);

            Assert.AreEqual(response["headers"]![HeaderName], headerValue);
        }
    }
}
