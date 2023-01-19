// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DragonFruit.Data.Basic;
using DragonFruit.Data.Parameters;
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

        [Test]
        public async Task TestHttp2Request()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://google.com") { Version = HttpVersion.Version20 };
            using var result = await Client.PerformAsync(request);

            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.AreEqual(request.Version, result.Version);
        }

        [Test]
        public void TestConcatEnumerable()
        {
            var req = new EnumerableTest(Enumerable.Range(1, 5)).FullUrl;

            Assert.True(req.Contains("sdata=1,2,3"));
            Assert.True(req.Contains("numbers=1,2,3"));
        }

        private class EnumerableTest : ApiRequest
        {
            public override string Path => "https://example.com";

            public EnumerableTest(IEnumerable<int> data)
            {
                Data = data;
            }

            [QueryParameter("numbers", CollectionConversionMode.Concatenated)]
            public IEnumerable<int> Data { get; set; }

            [QueryParameter("sdata", CollectionConversionMode.Concatenated)]
            public IEnumerable<string> StringData => Data.Select(x => x.ToString());
        }
    }
}
