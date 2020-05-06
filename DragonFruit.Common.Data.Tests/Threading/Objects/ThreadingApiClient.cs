// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Common.Data.Tests.Threading.Objects
{
    public class ThreadingApiClient : ApiClient
    {
        internal const string HeaderName = "x-dfn-test";

        public void ChangeHeaders(string value)
        {
            CustomHeaders.Clear();
            CustomHeaders.Add(HeaderName, value);
        }
    }
}
