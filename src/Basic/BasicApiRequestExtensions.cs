// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System.Collections.Generic;

#endregion

namespace DragonFruit.Data.Basic
{
    public static class BasicApiRequestExtensions
    {
        /// <summary>
        /// Appends a query parameter to the current <see cref="BasicApiRequest"/>
        /// </summary>
        public static T WithQuery<T>(this T request, string key, object value) where T : IBasicApiRequest
        {
            return request.WithQuery(key, value.ToString());
        }

        /// <summary>
        /// Appends a query parameter to the current <see cref="BasicApiRequest"/>
        /// </summary>
        public static T WithQuery<T>(this T request, string key, string value) where T : IBasicApiRequest
        {
            request.Queries.Value.Add(new KeyValuePair<string, string>(key, value));
            return request;
        }
    }
}
