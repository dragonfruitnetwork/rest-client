// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

#endregion

namespace DragonFruit.Data.Tests.Requests
{
    [TestFixture]
    public class StreamTests : ApiTest
    {
        [Test]
        public async Task TestStreamRequests()
        {
            var networkStream = await Client.PerformAsync<MemoryStream>("https://google.com");
            var fileStream = await Client.PerformAsync<FileStream>("https://google.com");

            Assert.AreEqual(networkStream.Length, fileStream.Length);
        }
    }
}
