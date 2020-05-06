// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Common.Data.Tests.Threading.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Data.Tests.Threading
{
    [TestClass]
    public class MultiThreadSyncTests
    {
        private const int MaxRequests = 10;

        [TestMethod]
        public void MultiThreadSyncTest()
        {
            var specialClient = new ThreadingApiClient();
            var echoRequest = new EchoRequest();
            var rng = new Random();

            Task PerformTest()
            {
                var headerValue = rng.Next().ToString();
                specialClient.ChangeHeaders(headerValue);

                var response = specialClient.Perform<JObject>(echoRequest);
                Assert.AreEqual(response["headers"]![ThreadingApiClient.HeaderName], headerValue);

                return null;
            }

            for (int i = 0; i < MaxRequests; i++)
            {
                Thread.Sleep(rng.Next(200, 500));
                PerformTest();
            }
        }
    }
}
