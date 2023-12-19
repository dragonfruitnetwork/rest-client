# DragonFruit.Data.Roslyn
A Roslyn source-generator and code-analyzer for DragonFruit.Data

[![Latest Nuget](https://img.shields.io/nuget/v/DragonFruit.Data.Roslyn?label=DragonFruit.Data.Roslyn&logo=nuget)](https://nuget.org/packages/DragonFruit.Data.Roslyn)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
DragonFruit.Data.Roslyn is a source-generator and code-analyzer for DragonFruit.Data that allows the request-building logic for `ApiRequest` classes to be generated at compile-time, rather than at runtime for each request.
It also provides code-analysis to ensure attributes are applied correctly and design rules are followed.

## Usage/Getting Started
**Note: while semantic versioning is used, it is best to ensure the versions of `DragonFruit.Data` and `DragonFruit.Data.Roslyn` are the same.**

The easiest way to get started is to install the [NuGet package](https://nuget.org/packages/DragonFruit.Data.Roslyn) alongside `DragonFruit.Data` and start writing `ApiRequest` classes.
The analyzer will inform you of any issues through your IDE.

Because the source generator writes code that is written at compile time, additional constraints are applied to `ApiRequest` classes:

- Classes must be marked with the `partial` keyword
- Classes must not be nested within another class
- Members cannot be marked as `private` or `internal` (only `public` and `protected` and `internal protected` are allowed)

Additionally, if a library uses the source generator, it must be added to all consumers unless all requests are `sealed`.
This is due to how the source generator works in combination with how DragonFruit.Data prioritises source-generated request builders.