// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Tests.Requests
{
    internal class FilteredRequest : ApiRequest
    {
        public override string Path { get; }

        protected override void OnRequestExecuting(ApiClient client)
        {
            throw new ArgumentException();
        }
    }

    internal class InheritedRequest : FilteredRequest
    {
        // this should have the exception filter applied to it as well
    }
}
