// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Common.Data.Tests.Handlers.AuthPreservingHandler.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonFruit.Common.Data.Tests.Handlers.AuthPreservingHandler
{
    [TestClass]
    public class HeaderPreservingHeaderTests
    {
        [TestMethod]
        public void TestHeaderPreservation()
        {
            var redirectClient = new HeaderPreservingHandlerClient();
            ApiRequest tokenRequest;

            try
            {
                tokenRequest = new AuthRequest();
            }
            catch
            {
                Assert.Inconclusive("not able to test due to inaccessible secrets.");
                return;
            }

            //get auth token
            var auth = redirectClient.Perform<BasicOrbitAuthResponse>(tokenRequest);
            redirectClient.Authorization = $"{auth.Type} {auth.AccessToken}";

            //user lookups by username = 301. without our HeaderPreservingHandler we'd get a 401
            redirectClient.Perform(new OrbitTestUserRequest());
        }
    }
}
