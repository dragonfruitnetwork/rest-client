// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Common.Data.Extensions
{
    public static class RequestExtensions
    {
        /// <summary>
        /// Sets the specified header for this request
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest"/> to set the header on</param>
        /// <param name="key">The header name</param>
        /// <param name="value">The header value</param>
        public static T WithHeader<T>(this T request, string key, string value) where T : ApiRequest
        {
            request.Headers.Value.Add(key, value);
            return request;
        }

        /// <summary>
        /// Sets the Authorization header for this request
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest"/> to set the header on</param>
        /// <param name="value">The auth header</param>
        public static T WithAuthenticationHeader<T>(this T request, string value) where T : ApiRequest
        {
            request.Headers.Value.Add("Authorization", value);
            return request;
        }
    }
}
