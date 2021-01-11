// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using DragonFruit.Common.Data.Basic;
using NUnit.Framework;

namespace DragonFruit.Common.Data.Tests.Files
{
    [TestFixture]
    public class FileDownloadTests : ApiTest
    {
        [TestCase("https://github.com/ppy/osu/archive/2020.1121.0.zip", 19018589)]
        public void FileDownloadTest(string path, long expectedFileSize)
        {
            var request = new BasicApiFileRequest(path, Path.GetTempPath());

            if (File.Exists(request.Destination))
            {
                try
                {
                    File.Delete(request.Destination);
                }
                catch
                {
                    Assert.Inconclusive("Failed to remove file needed for test");
                }
            }

            try
            {
                Client.Perform(request, (progress, total) => TestContext.Out.WriteLine($"Progress: {progress:n0}/{total:n0} ({Convert.ToSingle(progress) / Convert.ToSingle(total):F2}%)"));

                Assert.IsTrue(File.Exists(request.Destination));
                Assert.GreaterOrEqual(new FileInfo(request.Destination).Length, expectedFileSize);
            }
            finally
            {
                if (File.Exists(request.Destination))
                    File.Delete(request.Destination);
            }
        }
    }
}
