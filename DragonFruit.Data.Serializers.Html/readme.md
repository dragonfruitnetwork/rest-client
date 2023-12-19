# DragonFruit.Data.Serializers.Html
A serializer adding `HtmlDocument` support to DragonFruit.Data

[![Latest Nuget](https://img.shields.io/nuget/v/DragonFruit.Data.Serializers.Html?label=DragonFruit.Data.Serializers.Html&logo=nuget)](https://nuget.org/packages/DragonFruit.Data.Serializers.Html)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
DragonFruit.Data.Serializers.Html is a serializer for DragonFruit.Data that allows `HtmlDocument` to be used as a response type.
`HtmlDocument` functionality is provided by [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack), an open-source, MIT-licensed HTML parser.

## Usage/Getting Started
To get started install the [NuGet package](https://nuget.org/packages/DragonFruit.Data.Serializers.Html) alongside `DragonFruit.Data` and register it as a serializer.

```csharp
using DragonFruit.Data;
using DragonFruit.Data.Serializers.Html;

namespace DataExample;

public static class Program
{
    internal static ApiClient Client = new ApiClient<ApiJsonSerializer>
    {
        UserAgent = "DataExample"
    };

    static Program()
    {
        // register html serializer defaults
        HtmlSerializer.RegisterDefaults();
    }
    
    public static async Task Main(string[] args)
    {
        var html = await Client.PerformAsync<HtmlDocument>("https://example.com");
        
        // html can now be manipulated as needed
    }

}
```