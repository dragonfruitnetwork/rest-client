// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Globalization;
using System.IO;
using DragonFruit.Common.Data.Tests.FileDownloads;
using DragonFruit.Common.Data.Tests.SteamNews;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.Data.Tests
{
    [TestClass]
    public class TestBase
    {
        private static readonly ApiClient Client = new ApiClient(CultureInfo.InvariantCulture);

        [TestMethod]
        public void TestJsonRequest()
        {
            Client.Perform<SteamNewsResponse>(new SteamNewsRequest());
        }

        [TestMethod]
        public void TestFileRequest()
        {
            var request = new FileDownloadRequest();
            Client.Perform(request);

            File.Delete(request.Destination);
        }
    }
}