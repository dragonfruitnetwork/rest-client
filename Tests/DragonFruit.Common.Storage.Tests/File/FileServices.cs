// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.Storage.Tests.File
{
    [TestClass]
    public class FileServices
    {
        private readonly TestData _data = new TestData();
        private readonly string _tempFile = Path.GetTempFileName();

        [TestMethod]
        public void TestFileMethods()
        {
            Assert.IsTrue(new FileInfo(_tempFile).Length == 0);

            Storage.FileServices.WriteFile(_tempFile, _data);
            Assert.IsTrue(System.IO.File.Exists(_tempFile));

            var readData = Storage.FileServices.ReadFile<TestData>(_tempFile);
            Assert.IsTrue(_data.FirstName == readData.FirstName && _data.DoB == readData.DoB);
        }
    }
}