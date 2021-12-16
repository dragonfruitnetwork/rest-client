// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using DragonFruit.Data.Basic;
using DragonFruit.Data.Handlers;
using NUnit.Framework;

#pragma warning disable 618

namespace DragonFruit.Data.Tests.Handlers
{
    [TestFixture]
    public class InsecureSslVerificationHandlerTests : ApiTest
    {
        [Test]
        public void TestSslVerificationHandler()
        {
            var request = new BasicApiRequest("http://wrong.host.badssl.com/");

            try
            {
                Client.Perform(request);
                Assert.Fail("Request must fail when SSL validation is enabled");
            }
            // .NET Standard 2 returns aggregate exception
            catch (AggregateException e)
            {
                var innerExceptions = e.InnerExceptions.Select(x => x.InnerException);
                Assert.IsTrue(innerExceptions.Any(x => x is AuthenticationException));
            }
            // .NET 5 returns a non-aggregated copy of above
            catch (HttpRequestException e)
            {
                Assert.IsTrue(e.InnerException is AuthenticationException);
            }

            // set handler and go again
            Client.Handler = () => new InsecureSslVerificationHandler();
            Assert.IsTrue(Client.Perform(request).IsSuccessStatusCode);
        }
    }
}
