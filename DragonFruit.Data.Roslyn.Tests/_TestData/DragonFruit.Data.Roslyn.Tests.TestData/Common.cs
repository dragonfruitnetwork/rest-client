using System;

namespace DragonFruit.Data
{
    public class ApiRequest;
}

namespace DragonFruit.Data.Requests
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class RequestParameterAttribute : Attribute;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class RequestBodyAttribute : Attribute;
}
