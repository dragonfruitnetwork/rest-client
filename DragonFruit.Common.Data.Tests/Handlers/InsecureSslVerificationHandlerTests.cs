// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using DragonFruit.Common.Data.Basic;
using DragonFruit.Common.Data.Handlers;
using NUnit.Framework;

#pragma warning disable 618

namespace DragonFruit.Common.Data.Tests.Handlers
{
    [TestFixture]
    public class InsecureSslVerificationHandlerTests
    {
        [Test]
        public void TestSslVerificationHandler()
        {
            var client = new ApiClient();
            var request = new BasicApiRequest("http://wrong.host.badssl.com/");

            try
            {
                client.Perform(request);
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
            client.Handler = () => new InsecureSslVerificationHandler();
            Assert.IsTrue(client.Perform(request).IsSuccessStatusCode);
        }
    }
}
