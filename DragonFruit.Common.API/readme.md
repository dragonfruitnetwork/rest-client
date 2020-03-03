## DragonFruit.Common.API
> An API framework for effortless communication

### Usage:
> `T` is the class/datatype that represents the result

Steps:
- The class holding the request must inherit `ApiRequest` (see test model)
- A `new ApiClient()` must be created, but can be cached as a service
- The ApiClient calls for the ApiRequest to be performed

> note there are attributes, some of these are required. Examples to follow...