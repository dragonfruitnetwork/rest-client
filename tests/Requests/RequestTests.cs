// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Basic;
using DragonFruit.Data.Extensions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DragonFruit.Data.Tests.Requests
{
    [TestFixture]
    public class RequestTests : ApiTest
    {
        [TestCase(Methods.Put)]
        [TestCase(Methods.Post)]
        [TestCase(Methods.Patch)]
        [TestCase(Methods.Delete)]
        public void MethodWithBodyRequestTest(Methods method)
        {
            var request = new DatabaseUpdateRequest(method);
            var response = Client.Perform<JObject>(request);

            Assert.AreEqual(request.Employee.Department, response["json"].ToObject<Employee>().Department);
        }

        [TestCase]
        public void GetRequestTest()
        {
            var request = new SteamNewsRequest();

            //returns the data
            Client.Perform<JObject>(request);

            //returns just the response info - i.e a head request where serialization may not be desired
            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }

        [TestCase]
        public void BasicApiRequestTest()
        {
            var request = new BasicApiRequest("https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002")
                          .WithQuery("appid", 359550)
                          .WithQuery("count", 15);

            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }

        [TestCase]
        public void RawStringRequest()
        {
            var request = new DatabaseUpdateRequest(Methods.Post);
            var response = JObject.Parse(Client.Perform(request).To<string>());

            Assert.AreEqual(request.Employee.Department, response["json"].ToObject<Employee>().Department);
        }
    }
}
