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

            var firstResponse = Client.Perform<JObject>(request);
            Assert.AreEqual(request.Employee.Department, firstResponse["json"].ToObject<Employee>().Department);

            var secondResponse = Client.PerformLast<JObject>();
            Assert.AreEqual(secondResponse["json"].ToObject<Employee>().Department, firstResponse["json"].ToObject<Employee>().Department);
        }
    }
}
