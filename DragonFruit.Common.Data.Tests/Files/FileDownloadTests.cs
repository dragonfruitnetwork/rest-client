// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using DragonFruit.Common.Data.Tests.Files.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.Data.Tests.Files
{
    [TestClass]
    public class FileDownloadTests : ApiTest
    {
        [TestMethod]
        public void FileDownloadTest()
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
