// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using DragonFruit.Data.Parameters;

#endregion

namespace DragonFruit.Data.Tests.Requests
{
    internal class SteamNewsRequest : ApiRequest
    {
        public override string Path => "https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002";

        [QueryParameter("appid")]
        public int AppId { get; set; } = 359550;

        [QueryParameter("count")]
        public int MaxItems { get; set; } = 15;

        [QueryParameter("format")]
        public string Format { get; set; } = "json";

        [QueryParameter("maxlength")]
        public uint DescriptionLength { get; set; } = 90;
    }
}
