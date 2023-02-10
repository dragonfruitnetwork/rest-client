// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using DragonFruit.Data.Requests;
using NUnit.Framework;

namespace DragonFruit.Data.Tests.Requests
{
    [TestFixture]
    public class RequestFilterTests : ApiTest
    {
        [Test]
        public void TestFilteredRequests()
        {
            Assert.CatchAsync<ArgumentException>(() => Client.PerformAsync(new InheritedRequest()));
        }

        internal class FilteredRequest : ApiRequest, IRequestExecutingCallback
        {
            public override string Path { get; }

            void IRequestExecutingCallback.OnRequestExecuting(ApiClient client)
            {
                throw new ArgumentException();
            }
        }

        internal class InheritedRequest : FilteredRequest
        {
            // this should have the exception filter applied to it as well
        }
    }
}
