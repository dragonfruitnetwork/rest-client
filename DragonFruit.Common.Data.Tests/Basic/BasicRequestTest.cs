// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Linq;
using DragonFruit.Common.Data.Extensions;
using DragonFruit.Common.Data.Tests.Basic.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.Data.Tests.Basic
{
    [TestClass]
    public class BasicRequestTests : ApiTest
    {
        [TestMethod]
        public void ApiRequestTest()
        {
            var request = new SteamNewsRequest();

            //returns the data
            Client.Perform<SteamNewsResponse>(request);

            //returns just the response info - i.e a head request where serialization may not be desired
            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }

        [TestMethod]
        public void BasicApiRequestTest()
        {
            const int itemCount = 15;

            var request = new BasicApiRequest("https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002")
                          .WithQuery("appid", 359550)
                          .WithQuery("count", itemCount);

            Assert.AreEqual(itemCount, Client.Perform<SteamNewsResponse>(request).Container.NewsItems.Count());
            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }
    }
}
