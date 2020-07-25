// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using DragonFruit.Common.Data.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

#pragma warning disable 1998

namespace DragonFruit.Common.Data.Tests.Header
{
    [TestClass]
    public class HeaderTests : ApiTest
    {
        private const string HeaderName = "x-dfn-test";
        private const string GlobalHeaderName = "x-dfn-global";
        private static readonly Random Rng = new Random();

        [TestMethod]
        public void PerRequestHeaderTest()
        {
            var headerValue = Rng.Next().ToString();

            var request = new EchoRequest().WithHeader(HeaderName, headerValue);
            var response = Client.Perform<JObject>(request);

            Assert.AreEqual(response["headers"]![HeaderName], headerValue);
        }

        [TestMethod]
        public void LevelSpecificHeaderTest()
        {
            var request = new EchoRequest();

            var globalHeaderValue = Guid.NewGuid().ToString();
            var requestHeaderValue = Guid.NewGuid().ToString();

            Client.CustomHeaders.Add(GlobalHeaderName, globalHeaderValue);
            request.WithHeader(HeaderName, requestHeaderValue);

            var response = Client.Perform<JObject>(request);
            Assert.AreEqual(requestHeaderValue, response["headers"]![HeaderName]);
            Assert.AreEqual(globalHeaderValue, response["headers"]![GlobalHeaderName]);
        }

        [TestMethod]
        public void LevelOverrideHeaderTest()
        {
            var client = new ApiClient();
            var request = new EchoRequest();

            var globalHeaderValue = Guid.NewGuid().ToString();
            var requestHeaderValue = Guid.NewGuid().ToString();

            client.CustomHeaders.Add(GlobalHeaderName, globalHeaderValue);
            request.Headers.Value.Add(GlobalHeaderName, requestHeaderValue);

            var response = Client.Perform<JObject>(request);
            Assert.AreEqual(requestHeaderValue, response["headers"]![GlobalHeaderName]);
        }
    }
}
