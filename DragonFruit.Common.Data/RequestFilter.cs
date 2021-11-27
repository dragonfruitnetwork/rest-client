// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Common.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class RequestFilter : Attribute
    {
        protected internal abstract void OnRequestExecuting(ApiClient client, ApiRequest request);
    }
}
