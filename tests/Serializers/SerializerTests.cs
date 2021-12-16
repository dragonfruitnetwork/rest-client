// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using DragonFruit.Data.Basic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DragonFruit.Data.Tests.Serializers
{
    public class SerializerTests : ApiTest
    {
        private const int ArticleCount = 5;

        [Test]
        public async Task TestNewtonsoftSerializer()
        {
            var response = await Client.PerformAsync<JObject>(CreateRequest());
            var entries = response["appnews"]["newsitems"] as JArray;
            Assert.AreEqual(entries.Count, ArticleCount);
        }

        [Test]
        public async Task TestSystemTextJsonSerializer()
        {
            var response = await Client.PerformAsync<JsonDocument>(CreateRequest());
            var articleCount = response.RootElement.GetProperty("appnews").GetProperty("newsitems").GetArrayLength();

            Assert.AreEqual(articleCount, ArticleCount);
        }

        [Test]
        public async Task TestXmlSerializer()
        {
            var response = await Client.PerformAsync<XmlDocument>(CreateRequest("xml"));
            var articleNodes = response.SelectNodes("/appnews/newsitems/newsitem");

            Assert.AreEqual(articleNodes?.Count, ArticleCount);
        }

        private ApiRequest CreateRequest(string format = "json") => new BasicApiRequest("https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/")
                                                                    .WithQuery("appid", "440")
                                                                    .WithQuery("count", ArticleCount)
                                                                    .WithQuery("maxlength", "300")
                                                                    .WithQuery("format", format);
    }
}
