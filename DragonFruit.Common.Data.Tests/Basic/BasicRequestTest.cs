// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Common.Data.Tests.Basic.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.Data.Tests.Basic
{
    [TestClass]
    public class BasicRequestTests : ApiTest
    {
        [TestMethod]
        public void BasicRequestTest()
        {
            var request = new SteamNewsRequest();

            //returns the data
            Client.Perform<SteamNewsResponse>(request);

            //returns just the response info - i.e a head request where serialization may not be desired
            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }
    }
}
