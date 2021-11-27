// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Common.Data.Tests.Requests
{
    [ExceptionFilter]
    internal class FilteredRequest : ApiRequest
    {
        public override string Path { get; }
    }

    internal class InheritedRequest : FilteredRequest
    {
        // this should have the exception filter applied to it as well
    }

    internal class ExceptionFilter : RequestFilter
    {
        protected override void OnRequestExecuting(ApiClient client, ApiRequest request)
        {
            throw new ArgumentException();
        }
    }
}
