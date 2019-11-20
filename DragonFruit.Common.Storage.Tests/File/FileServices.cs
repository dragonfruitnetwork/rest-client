// DragonFruit.Common Copyright 2019 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using DragonFruit.Common.Storage.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.Storage.File.Tests
{
    [TestClass]
    public class FileServices
    {
        private readonly string _tempFile = Path.GetTempFileName();
        private readonly TestData _data = new TestData();

        [TestMethod]
        public void TestFileMethods()
        {
            Assert.IsTrue(new FileInfo(_tempFile).Length == 0);

            File.FileServices.WriteFile(_tempFile, _data);
            Assert.IsTrue(System.IO.File.Exists(_tempFile));

            var readData = File.FileServices.ReadFile<TestData>(_tempFile);
            Assert.IsTrue(_data.FirstName == readData.FirstName && _data.DoB == readData.DoB);
        }
    }
}