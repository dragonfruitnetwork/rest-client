# DragonFruit.Data.Serializers.Newtonsoft
A serializer adding `Newtonsoft.Json` support to DragonFruit.Data

[![Latest Nuget](https://img.shields.io/nuget/v/DragonFruit.Data.Serializers.Newtonsoft?label=DragonFruit.Data.Serializers.Newtonsoft&logo=nuget)](https://nuget.org/packages/DragonFruit.Data.Serializers.Newtonsoft)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
DragonFruit.Data.Serializers.Newtonsoft is a generic serializer for DragonFruit.Data that allows Newtonsoft.Json to be used as a generic JSON serializer.

## Usage/Getting Started
To get started install the [NuGet package](https://nuget.org/packages/DragonFruit.Data.Serializers.Newtonsoft) and replace the default serializer with the Newtonsoft one.

```csharp
using DragonFruit.Data;
using DragonFruit.Data.Serializers.Newtonsoft;

namespace DataExample;

public static class Program
{
    internal static ApiClient Client = new ApiClient<NewtonsoftJsonSerializer>
    {
        UserAgent = "DataExample"
    };

    static Program()
    {
        // optionally, register serializer defaults to always deserialize JObjects with Newtonsoft
        // using this will allow for a different serializer to be used for other types, while not breaking JObject support.
        NewtonsoftJsonSerializer.RegisterDefaults();
    }
}
```