// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Threading.Tasks;
using DragonFruit.Data.Basic;
using NUnit.Framework;

namespace DragonFruit.Data.Tests.Requests
{
    [TestFixture]
    public class RequestTests : ApiTest
    {
        [TestCase(Methods.Get)]
        [TestCase(Methods.Post)]
        [TestCase(Methods.Head)]
        [TestCase(Methods.Patch)]
        [TestCase(Methods.Delete)]
        public async Task TestMethodRequest(Methods method)
        {
            var request = new EchoRequest(method);
            using var result = await Client.PerformAsync(request);

            Assert.IsTrue(result.IsSuccessStatusCode);
        }

        [Test]
        public async Task TestBasicRequest()
        {
            var request = new BasicApiRequest("https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/")
                          .WithQuery("appid", "440")
                          .WithQuery("count", "3")
                          .WithQuery("maxlength", "300")
                          .WithQuery("format", "json");

            using var result = await Client.PerformAsync(request);

            Assert.IsTrue(result.IsSuccessStatusCode);
        }
    }
}
