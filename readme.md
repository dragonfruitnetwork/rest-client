# DragonFruit.Data
A lightweight, extensible HTTP/REST framework for .NET

[![Latest Nuget](https://img.shields.io/nuget/v/DragonFruit.Data?label=DragonFruit.Data&logo=nuget)](https://nuget.org/packages/DragonFruit.Data)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

### Overview
DragonFruit.Data is a HTTP REST client for .NET that is designed to be easy to use and acts as the main web communication system for many DragonFruit products, including internal tools.

### Usage/Getting Started
The easiest way to get started is to install the [NuGet package](https://nuget.org/packages/DragonFruit.Data), create an `ApiClient` and start making requests.
For more information, see the [wiki](https://dragonfruit.network/wiki/rest-client) and the [getting started guide](https://dragonfruit.network/wiki/rest-client/getting-started).

#### `SteamRequest.cs`

```csharp
using DragonFruit.Data;
using DragonFruit.Data.Requests;

namespace DataExample;

public partial class SteamNewsRequest : ApiRequest
{
    public override string RequestPath => "https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002";

    public SteamNewsRequest(int appId)
    {
        AppId = appId;
    }

    [RequestParameter(ParameterType.Query, "appid")]
    public int AppId { get; set; }

    [RequestParameter(ParameterType.Query, "count")]
    public int? Count { get; set; }

    [RequestParameter(ParameterType.Query, "maxlength")]
    public int? MaxLength { get; set; }

    [RequestParameter(ParameterType.Query, "format")]
    protected string Format => "json";
}

```

#### `Program.cs`

```csharp
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.Data.Serializers;

namespace DataExample;

public class Program 
{
    internal static ApiClient Client = new ApiClient<ApiJsonSerializer>
    {
        UserAgent = "DataExample"
    };

    public static async Task Main(string[] args)
    {
        var tf2NewsRequest = new SteamNewsRequest(440);
        var tf2News = await Client.PerformAsync<JsonObject>(tf2NewsRequest);
        
        // tf2News is now a JsonObject that can be manipulated as needed
    }
}
```

### License
The project is licensed under MIT. Refer to [license.md](license.md) for more information.