// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Common.Data.Exceptions;

namespace DragonFruit.Common.Data
{
    public partial class ApiClient
    {
        /// <summary>
        /// Perform a request to the specified <see cref="url"/> that returns a strongly-typed class.
        /// </summary>
        public Task<T> PerformAsync<T>(string url, CancellationToken token = default) where T : class
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return PerformAsync<T>(request, token);
        }

        /// <summary>
        /// Perform an <see cref="ApiRequest"/> with a specified return type.
        /// </summary>
        public Task<T> PerformAsync<T>(ApiRequest requestData, CancellationToken token = default) where T : class
        {
            ValidateRequest(requestData);
            return PerformAsync<T>(requestData.Build(this), token);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/> and deserialize the result to the specified type
        /// </summary>
        public Task<T> PerformAsync<T>(HttpRequestMessage request, CancellationToken token = default) where T : class
        {
            return InternalPerform(request, ValidateAndProcess<T>, true, token);
        }

        /// <summary>
        /// Perform a request to the specified <see cref="url"/> that returns a <see cref="HttpResponseMessage"/>.
        /// </summary>
        public Task<HttpResponseMessage> PerformAsync(string url, CancellationToken token = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return PerformAsync(request, token);
        }

        /// <summary>
        /// Perform a <see cref="ApiRequest"/> that returns the response message.
        /// </summary>
        public Task<HttpResponseMessage> PerformAsync(ApiRequest requestData, CancellationToken token = default)
        {
            ValidateRequest(requestData);
            return PerformAsync(requestData.Build(this), token);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/>
        /// </summary>
        public Task<HttpResponseMessage> PerformAsync(HttpRequestMessage request, CancellationToken token = default)
        {
            return InternalPerform(request, Task.FromResult, false, token);
        }

        /// <summary>
        /// Download a file with an <see cref="ApiRequest"/>
        /// </summary>
        /// <remarks>
        /// Bypasses <see cref="ValidateAndProcess{T}"/>
        /// </remarks>
        public Task PerformAsync(ApiFileRequest request, Action<long, long?> progressUpdated = null, CancellationToken token = default)
        {
            // check request data is valid
            ValidateRequest(request);

            if (string.IsNullOrWhiteSpace(request.Destination))
            {
                throw new NullRequestException();
            }

            async Task<HttpResponseMessage> CopyProcess(HttpResponseMessage response)
            {
                // validate
                response.EnsureSuccessStatusCode();

                // create a new filestream and copy all data into
                using var stream = File.Open(request.Destination, request.FileCreationMode);
                using var networkStream = await response.Content.ReadAsStreamAsync();

                // rent a buffer for progress reporting
                var buffer = ArrayPool<byte>.Shared.Rent(request.BufferSize);
                int count;
                int iterations = 0;

                while ((count = await networkStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    Interlocked.Increment(ref iterations);
                    await stream.WriteAsync(buffer, 0, count, token);

                    // check every 10th time to stop bottlenecks (use CompareExchange to stop the int from overflowing from insanely large file downloads)
                    if (Interlocked.CompareExchange(ref iterations, 0, 10) == 10)
                        progressUpdated?.Invoke(stream.Length, response.Content.Headers.ContentLength);
                }

                // flush, return buffer and send a final update
                await stream.FlushAsync(token);
                ArrayPool<byte>.Shared.Return(buffer);

                progressUpdated?.Invoke(stream.Length, response.Content.Headers.ContentLength);
                return response;
            }

            return InternalPerform(request.Build(this), CopyProcess, true, token);
        }
    }
}
