// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;
using NUnit.Framework;

#endregion

namespace DragonFruit.Data.Tests.Requests
{
    [TestFixture]
    public class RequestFilterTests : ApiTest
    {
        [Test]
        public void TestFilteredRequests()
        {
            Assert.Catch<ArgumentException>(() => Client.Perform(new FilteredRequest()));
            Assert.Catch<ArgumentException>(() => Client.Perform(new InheritedRequest()));
        }
    }
}
