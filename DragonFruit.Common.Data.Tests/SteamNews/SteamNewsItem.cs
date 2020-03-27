// Dragon6 Mobile Copyright 2020 © DragonFruit Network

using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Tests.SteamNews
{
    public class SteamNewsItem
    {
        [JsonProperty("gid")] public string ID { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("url")] public string Link { get; set; }

        [JsonProperty("date")] public double Epoch { get; set; }

        [JsonProperty("author")] public string Author { get; set; }

        [JsonProperty("feedlabel")] public string Feed { get; set; }
    }
}