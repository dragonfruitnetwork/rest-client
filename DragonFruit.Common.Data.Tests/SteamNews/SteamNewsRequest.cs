// Dragon6 Mobile Copyright 2020 © DragonFruit Network

using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.Common.Data.Tests.SteamNews
{
    public class SteamNewsRequest : ApiRequest
    {
        public override string Path => "https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002";

        [QueryParameter("appid")] public int AppId { get; set; } = 359550;

        [QueryParameter("count")] public int MaxItems { get; set; } = 15;

        [QueryParameter("format")] public string Format { get; set; } = "json";

        [QueryParameter("maxlength")] public uint DescriptionLength { get; set; } = 90;
    }
}