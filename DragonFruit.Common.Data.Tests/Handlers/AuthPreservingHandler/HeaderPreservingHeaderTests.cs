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

            var auth = redirectClient.Perform<BasicOrbitAuthResponse>(new AuthRequest());
            redirectClient.Authorization = $"{auth.Type} {auth.AccessToken}";

            //user lookups by username = 301. without our HeaderPreservingHandler we'd get a 401
            redirectClient.Perform(new OrbitTestUserRequest());
        }
    }
}
