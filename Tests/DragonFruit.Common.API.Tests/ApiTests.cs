// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using DragonFruit.Common.API.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.API.Tests
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void TestApi()
        {
            var request = new Request
            {
                AppId = 359550,
                Count = 15,
                MaxLength = 90
            };

            var client = new ApiClient();

            client.Perform<IEnumerable<Response>>(request);
        }
    }
}