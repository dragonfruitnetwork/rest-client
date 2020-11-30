// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using DragonFruit.Common.Data.Basic;
using NUnit.Framework;

namespace DragonFruit.Common.Data.Tests.Files
{
    [TestFixture]
    public class FileDownloadTests : ApiTest
    {
        [TestCase]
        public void FileDownloadTest()
        {
            var request = new BasicApiFileRequest("https://github.com/ppy/osu/archive/2020.1121.0.zip", Path.GetTempPath());

            Assert.IsFalse(File.Exists(request.Destination));

            try
            {
                Client.Perform(request);

                Assert.IsTrue(File.Exists(request.Destination));
                Assert.IsTrue(new FileInfo(request.Destination).Length > 5000);
            }
            finally
            {
                if (File.Exists(request.Destination))
                    File.Delete(request.Destination);
            }
        }
    }
}
