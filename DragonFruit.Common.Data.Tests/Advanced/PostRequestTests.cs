// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Common.Data.Tests.Advanced.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Data.Tests.Advanced
{
    [TestClass]
    public class PostRequestTests : ApiTest
    {
        [TestMethod]
        public void JsonPostRequestTest()
        {
            var request = new DatabaseUpdateRequest();

            var response = Client.Perform<JObject>(request);
            Assert.AreEqual(request.Employee.Department, response["json"].ToObject<Employee>().Department);
        }
    }
}
