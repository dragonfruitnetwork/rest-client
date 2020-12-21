// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using DragonFruit.Common.Data.Handlers;

namespace DragonFruit.Common.Data.Tests.Handlers.AuthPreservingHandler
{
    public class HeaderPreservingHandlerClient : ApiClient
    {
        protected override HttpMessageHandler CreateHandler() => new HeaderPreservingRedirectHandler();
    }
}
