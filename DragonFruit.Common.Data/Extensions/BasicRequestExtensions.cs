// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;

namespace DragonFruit.Common.Data.Extensions
{
    public static class BasicRequestExtensions
    {
        /// <summary>
        /// Appends a query parameter to the current <see cref="BasicApiRequest"/>
        /// </summary>
        public static BasicApiRequest WithQuery(this BasicApiRequest request, string key, object value)
        {
            return request.WithQuery(key, value.ToString());
        }

        /// <summary>
        /// Appends a query parameter to the current <see cref="BasicApiRequest"/>
        /// </summary>
        public static BasicApiRequest WithQuery(this BasicApiRequest request, string key, string value)
        {
            request.Queries.Value.Add(new KeyValuePair<string, string>(key, value));
            return request;
        }
    }
}
