// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Tests.Handlers.AuthPreservingHandler.Requests
{
    public class OrbitAuthResponse
    {
        [JsonProperty("token_type")]
        public string Type { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
