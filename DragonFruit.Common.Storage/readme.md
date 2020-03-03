## DragonFruit.Common.Storage
> An easy to use Web/File based access and storage methods.

### Files:
> In each use case, `T` is the class/datatype being stored

> Add the `using` statement: `using DragonFruit.Common.Storage.File` to the top of each file using these libraries

|Read|Write|
|--|--|
|`var file = FileServices.ReadFile<T>(@"C:\Users\Demo\Desktop\demofile.info");`|`FileServices.WriteFile(@"C:\Users\Demo\Desktop\demofile.info", data);`|

### Web:
> In each use case, `T` is the class/datatype being stored and `url` is a link that points to a JSON-based response string

> Add the `using` statement: `using DragonFruit.Common.Storage.Web` to the top of each file using these libraries

|Read as Object|Read as JObject| 
|--|--|
|`var data = WebServices.StreamObject<T>(url);`|`var jData = WebServices.StreamJObject(url);`|