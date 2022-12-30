// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Net.Http;
using System.Threading;

namespace DragonFruit.Data
{
    public partial class ApiClient
    {
        private const string SyncObsoletionMessage = "Synchronous client operations will be removed in a future update. Change to Async variations where possible or block with .Wait()/.Result";

        /// <summary>
        /// Perform a request to the specified <see cref="url"/> that returns a strongly-typed class.
        /// </summary>
        [Obsolete(SyncObsoletionMessage)]
        public T Perform<T>(string url, CancellationToken token = default) where T : class
        {
            return PerformAsync<T>(url, token).Result;
        }

        /// <summary>
        /// Perform an <see cref="ApiRequest"/> with a specified return type.
        /// </summary>
        [Obsolete(SyncObsoletionMessage)]
        public T Perform<T>(ApiRequest requestData, CancellationToken token = default) where T : class
        {
            return PerformAsync<T>(requestData, token).Result;
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/> and deserialize the result to the specified type
        /// </summary>
        [Obsolete(SyncObsoletionMessage)]
        public T Perform<T>(HttpRequestMessage request, CancellationToken token = default) where T : class
        {
            return PerformAsync<T>(request, token).Result;
        }

        /// <summary>
        /// Perform a request to the specified <see cref="url"/> that returns a <see cref="HttpResponseMessage"/>.
        /// </summary>
        [Obsolete(SyncObsoletionMessage)]
        public HttpResponseMessage Perform(string url, CancellationToken token = default)
        {
            return PerformAsync(url, token).Result;
        }

        /// <summary>
        /// Perform a <see cref="ApiRequest"/> that returns the response message.
        /// </summary>
        [Obsolete(SyncObsoletionMessage)]
        public HttpResponseMessage Perform(ApiRequest requestData, CancellationToken token = default)
        {
            return PerformAsync(requestData, token).Result;
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/>
        /// </summary>
        [Obsolete(SyncObsoletionMessage)]
        public HttpResponseMessage Perform(HttpRequestMessage request, CancellationToken token = default)
        {
            return PerformAsync(request, token).Result;
        }

        /// <summary>
        /// Download a file with an <see cref="ApiRequest"/>
        /// </summary>
        /// <remarks>
        /// Bypasses <see cref="ValidateAndProcess{T}"/>
        /// </remarks>
        [Obsolete(SyncObsoletionMessage)]
        public void Perform(ApiFileRequest request, Action<long, long?> progressUpdated = null, CancellationToken token = default)
        {
            PerformAsync(request, progressUpdated, token).Wait(token);
        }
    }
}
