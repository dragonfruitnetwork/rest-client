// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Linq;
using DragonFruit.Common.Data.Extensions;
using DragonFruit.Common.Data.Tests.Requests.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Data.Tests.Requests
{
    [TestClass]
    public class RequestTests : ApiTest
    {
        [TestMethod]
        public void MethodWithBodyRequestTest()
        {
            foreach (var requestMethod in Enum.GetValues(typeof(Methods)).Cast<Methods>())
            {
                if (requestMethod == Methods.Get || requestMethod == Methods.Head || requestMethod == Methods.Trace)
                {
                    continue;
                }

                var request = new DatabaseUpdateRequest(requestMethod);
                var response = Client.Perform<JObject>(request);

                Assert.AreEqual(request.Employee.Department, response["json"].ToObject<Employee>().Department);
            }
        }

        [TestMethod]
        public void GetRequestTest()
        {
            var request = new SteamNewsRequest();

            //returns the data
            Client.Perform<JObject>(request);

            //returns just the response info - i.e a head request where serialization may not be desired
            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }

        [TestMethod]
        public void BasicApiRequestTest()
        {
            var request = new BasicApiRequest("https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002")
                          .WithQuery("appid", 359550)
                          .WithQuery("count", 15);

            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }
    }
}
