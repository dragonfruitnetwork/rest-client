// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using Newtonsoft.Json;

namespace DragonFruit.Common.API.Tests.Models
{
    public class Response
    {
        [JsonProperty("gid")]
        public string ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Link { get; set; }

        [JsonProperty("date")]
        public double Epoch { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("feedlabel")]
        public string Feed { get; set; }
    }
}