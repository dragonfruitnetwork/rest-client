// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Globalization;
using System.IO;
using DragonFruit.Common.Data.Tests.BodySerialization;
using DragonFruit.Common.Data.Tests.FileDownloads;
using DragonFruit.Common.Data.Tests.SteamNews;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Data.Tests
{
    [TestClass]
    public class TestBase
    {
        private static readonly ApiClient Client = new ApiClient(CultureInfo.InvariantCulture);

        [TestMethod]
        public void TestJsonRequest()
        {
            var request = new SteamNewsRequest();

            //returns the data
            Client.Perform<SteamNewsResponse>(request);

            //returns just the response info - i.e a head request where serialization may not be desired
            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }

        [TestMethod]
        public void TestJsonBodyRequest()
        {
            var request = new DatabaseUpdateRequest();

            var firstResponse = Client.Perform<JObject>(request);
            Assert.AreEqual(request.Employee.Department, firstResponse["json"].ToObject<Employee>().Department);

            var secondResponse = Client.PerformLast<JObject>();
            Assert.AreEqual(secondResponse["json"].ToObject<Employee>().Department, firstResponse["json"].ToObject<Employee>().Department);
        }

        [TestMethod]
        public void TestFileRequest()
        {
            var request = new FileDownloadRequest();

            Assert.IsFalse(File.Exists(request.Destination));

            try
            {
                Client.Perform(request);

                Assert.IsTrue(File.Exists(request.Destination));
                Assert.IsTrue(new FileInfo(request.Destination).Length > 5000);

                File.Delete(request.Destination);
            }
            catch
            {
                if (File.Exists(request.Destination))
                    File.Delete(request.Destination);

                throw;
            }
        }
    }
}
