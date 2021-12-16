# DragonFruit.Data

![CI Publish](https://github.com/dragonfruitnetwork/DragonFruit.Common/workflows/Publish/badge.svg)
![CI Unit Tests](https://github.com/dragonfruitnetwork/DragonFruit.Common/workflows/Unit%20Tests/badge.svg)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/55343888c7e945b3b7d9d4760309ccb4)](https://www.codacy.com/gh/dragonfruitnetwork/dragonfruit.common?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=dragonfruitnetwork/DragonFruit.Common&amp;utm_campaign=Badge_Grade)
[![Nuget](https://img.shields.io/nuget/v/DragonFruit.Common.Data)](https://nuget.org/packages/DragonFruit.Common.Data)
![Nuget Downloads](https://img.shields.io/nuget/dt/DragonFruit.Common.Data)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

### Overview

DragonFruit.Data provides the framework that all DragonFruit Apps and APIs rely on to deliver data from remote sources. This framework has been created with 5 points in mind:

- Streamlined - Developers create a central `ApiClient` and pass `ApiRequest`s to be performed
- Optimised - All official `ApiSerializer`s have been optimised for performance and memory usage
- Expandable - `ApiSerializer` is abstract, allowing for custom serializers to be created
- Ease of use - Requests use attributes which makes querystring and form parameter mapping easy
- Integration - `ApiClient`s integrate with existing systems, including `HttpRequestMessage`/`HttpResponseMessage` and `HttpMessageHandler`

### Getting Started

> The docs are slightly out of date, and we'll be revising them soon to bring them up-to-date

See the [wiki](https://github.com/dragonfruitnetwork/DragonFruit.Common/wiki) to get started.
