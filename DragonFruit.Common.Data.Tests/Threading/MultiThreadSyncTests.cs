// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Common.Data.Tests.Threading.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

#pragma warning disable 1998

namespace DragonFruit.Common.Data.Tests.Threading
{
    [TestClass]
    public class MultiThreadSyncTests
    {
        private const int Requests = 10;

        [TestMethod]
        public void MultiThreadSyncTest()
        {
            var specialClient = new ThreadingApiClient();
            var echoRequest = new EchoRequest();
            var rng = new Random();

            async Task PerformTest()
            {
                var headerValue = rng.Next().ToString();
                specialClient.ChangeHeaders(headerValue);

                var response = specialClient.Perform<JObject>(echoRequest);
                Assert.AreEqual(response["headers"]![ThreadingApiClient.HeaderName], headerValue);
            }

            for (int i = 0; i < Requests; i++)
            {
                Thread.Sleep(rng.Next(200, 500));
                _ = PerformTest();
            }
        }
    }
}
