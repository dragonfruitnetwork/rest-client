# DragonFruit.Common

## Overview
DragonFruit.Common is aimed to reduce code repetition and make common tasks easier. Additionally, the code is Memory-aware and will reduce leaks in code.
 All Component Packages are licensed under MIT. Please refer to the `LICENSE` file for more information

# Components

In order to use a component, you must install each one separately from NuGet (Note more are in development but are not finished yet):

|Name|Link|Description
|--|--|--|
|`DragonFruit.Common.Storage`|[NuGet](https://www.nuget.org/packages/DragonFruit.Common.Storage)|Easy to use Web/File based access and storage methods.|

## DragonFruit.Common.API
> API framework for effortless communication

### Files:
> `T` is the class/datatype that represents the result

Steps:
- The class holding the request must inherit `ApiRequest` (see )
- A `new ApiClient()` must be created, but can be cached as a service
- The ApiClient calls for the ApiRequest to be performed