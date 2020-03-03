// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Common.API.Attributes;
using DragonFruit.Common.API.Enums;

namespace DragonFruit.Common.API.Tests.Models
{
    [ApiPath("https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/", Methods.Get)]
    public class Request : ApiRequest
    {
        [QueryParameter("appid")]
        public uint AppId { get; set; }

        [QueryParameter("count")]
        public uint Count { get; set; }

        [QueryParameter("maxlength")]
        public uint MaxLength { get; set; }

        [QueryParameter("format")]
        public string Format => "json";
    }
}