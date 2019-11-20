# DragonFruit.Common

## Overview
DragonFruit.Common is aimed to reduce code repetition and make common tasks easier. Additionally, the code is Memory-aware and will reduce leaks in code.
 All Component Packages are licensed under MIT. Please refer to the `LICENSE` file for more information

# Components

In order to use a component, you must install each one separately from NuGet (Note more are in development but are not finished yet):

|Name|Link|Description
|--|--|--|
|`DragonFruit.Common.Storage`|[NuGet](https://www.nuget.org/packages/DragonFruit.Common.Storage)|Easy to use Web/File based access and storage methods.|

## DragonFruit.Common.Storage
> An easy to use Web/File based access and storage methods.

### Files:
> In each use case, `T` is the class/datatype being stored

> Add the `using` statement: `using DragonFruit.Common.Storage.File` to the top of each file using these libraries

|Read|Write|
|--|--|
|`var file = FileServices.ReadFile<T>(@"C:\Users\Demo\Desktop\demofile.info");`|`FileServices.WriteFile<T>(@"C:\Users\Demo\Desktop\demofile.info", data);`|

### Web:
> In each use case, `T` is the class/datatype being stored and `url` is a link that points to a JSON-based response string

> Add the `using` statement: `using DragonFruit.Common.Storage.Web` to the top of each file using these libraries

|Read as Object|Read as JObject| 
|--|--|
|`var data = WebServices.StreamObject<T>(url);`|`var jData = WebServices.StreamJObject(url);`|
