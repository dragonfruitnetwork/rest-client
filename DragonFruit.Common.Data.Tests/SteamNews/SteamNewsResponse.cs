// Dragon6 Mobile Copyright 2020 © DragonFruit Network

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