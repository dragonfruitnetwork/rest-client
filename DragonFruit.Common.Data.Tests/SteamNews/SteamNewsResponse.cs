// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Tests.SteamNews
{
    public class SteamNewsResponse
    {
        [JsonProperty("appnews")] public SteamAppNewsContainer Container { get; set; }
    }

    public class SteamAppNewsContainer
    {
        [JsonProperty("appid")] public int AppId { get; set; }

        [JsonProperty("newsitems")] public IEnumerable<SteamNewsItem> NewsItems { get; set; }
    }
}