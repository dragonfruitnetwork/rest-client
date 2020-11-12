// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DragonFruit.Common.Data.Utils
{
    /// <summary>
    /// async wrappers for <see cref="ApiClient"/>
    /// </summary>
    public static class AsyncClientUtils
    {
        /// <summary>
        /// Perform a <see cref="ApiRequest"/> that returns the response message. The <see cref="HttpResponseMessage"/> returned cannot be used for reading data, as the underlying <see cref="Task"/> will be disposed.
        /// </summary>
        public static Task<HttpResponseMessage> PerformAsync<TClient>(this TClient client, ApiRequest requestData, CancellationToken token = default)
            where TClient : ApiClient
        {
            return Task.Run(() => client.Perform(requestData, token), token);
        }

        /// <summary>
        /// Perform an <see cref="ApiRequest"/> with a specified return type.
        /// </summary>
        public static Task<T> PerformAsync<TClient, T>(this TClient client, ApiRequest requestData, CancellationToken token = default)
            where TClient : ApiClient
            where T : class
        {
            return Task.Run(() => client.Perform<T>(requestData, token), token);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/>
        /// </summary>
        public static Task<HttpResponseMessage> PerformAsync<TClient>(this TClient client, HttpRequestMessage request, CancellationToken token = default)
            where TClient : ApiClient
        {
            return Task.Run(() => client.Perform(request, token), token);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/> and deserialize the result to the specified type
        /// </summary>
        public static Task<T> PerformAsync<TClient, T>(this TClient client, HttpRequestMessage request, CancellationToken token = default)
            where T : class
            where TClient : ApiClient
        {
            return Task.Run(() => client.Perform<T>(request, token), token);
        }

        /// <summary>
        /// Download a file with an <see cref="ApiRequest"/>. Bypasses <see cref="ValidateAndProcess{T}"/>
        /// </summary>
        public static Task PerformAsync<TClient>(this TClient client, ApiFileRequest request, CancellationToken token = default)
            where TClient : ApiClient
        {
            return Task.Run(() => client.Perform(request, token), token);
        }
    }
}
