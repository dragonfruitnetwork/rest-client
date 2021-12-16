// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;
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

            // make sure stream lengths are _almost_ the same length
            var difference = Math.Abs(networkStream.Length - fileStream.Length);
            Assert.IsTrue(difference <= 10);
        }
    }
}
