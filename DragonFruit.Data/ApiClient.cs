// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using DragonFruit.Data.Converters;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Serializers;

namespace DragonFruit.Data
{
    /// <summary>
    /// Represents a strongly-typed serializer version of <see cref="ApiClient"/>
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ApiSerializer"/></typeparam>
    public class ApiClient<T> : ApiClient where T : ApiSerializer, new()
    {
        public ApiClient()
            : base(new T())
        {
        }

        public ApiClient(Uri baseAddress)
            : base(new T(), baseAddress)
        {
        }
    }

    /// <summary>
    /// The <see cref="ApiClient"/> responsible for building, submitting and processing HTTP requests
    /// </summary>
    public class ApiClient(ApiSerializer serializer)
    {
        private HttpClient? _client;
        private Uri? _baseAddress;

        public ApiClient(ApiSerializer serializer, Uri baseAddress)
            : this(serializer)
        {
            _baseAddress = baseAddress;
        }

        ~ApiClient()
        {
            _client?.Dispose();
        }

        static ApiClient()
        {
            // register generic xml document type
            SerializerResolver.Register<XmlDocument, ApiXmlSerializer>();

            // register system.text.json types
            SerializerResolver.Register<JsonObject, ApiJsonSerializer>();
            SerializerResolver.Register<JsonDocument, ApiJsonSerializer>();

            // register stream resolver types (inwards only)
            SerializerResolver.Register<Stream, InternalStreamSerializer>(DataDirection.In);
            SerializerResolver.Register<FileStream, InternalStreamSerializer>(DataDirection.In);
            SerializerResolver.Register<MemoryStream, InternalStreamSerializer>(DataDirection.In);
        }

        /// <summary>
        /// Gets or sets the User-Agent used in HTTP requests
        /// </summary>
        public string UserAgent
        {
            get => Client.DefaultRequestHeaders.UserAgent.ToString();
            set
            {
                Client.DefaultRequestHeaders.UserAgent.Clear();
                Client.DefaultRequestHeaders.UserAgent.ParseAdd(value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> that should act as the base address for relative-URI requests
        /// </summary>
        public Uri? BaseAddress
        {
            get => _client?.BaseAddress ?? _baseAddress;
            set
            {
                _baseAddress = value;
                _client?.BaseAddress = value;
            }
        }

        /// <summary>
        /// The <see cref="SerializerResolver"/> instance used to resolve serializers for requests.
        /// Caches and reused serializers where possible.
        /// </summary>
        public SerializerResolver Serializers { get; } = new(serializer);

        /// <summary>
        /// User-Controlled method to create a <see cref="HttpMessageHandler"/>
        /// </summary>
        public Func<HttpMessageHandler>? Handler { get; set; }

        /// <summary>
        /// Gets the header container for the underlying <see cref="HttpClient"/>
        /// </summary>
        public HttpRequestHeaders Headers => Client.DefaultRequestHeaders;

        /// <summary>
        /// Gets the <see cref="HttpClient"/> used across all requests.
        /// </summary>
        protected HttpClient Client => _client ??= CreateClient();

        /// <summary>
        /// Sends a GET request to the provided <see cref="url"/>, returning a deserialized response.
        /// </summary>
        public Task<T> PerformAsync<T>(string url, CancellationToken cancellationToken = default) where T : class
        {
            var serializer = Serializers.Resolve<T>(DataDirection.In);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(serializer.ContentType));

            return PerformAsyncInternal<T>(requestMessage, serializer, cancellationToken);
        }

        /// <summary>
        /// Builds and performs an <see cref="ApiRequest"/>, deserializing the results into the specified type.
        /// </summary>
        /// <remarks>
        /// If source generation is enabled, the source generated method will be used to build the request, otherwise a legacy reflection handler will build the request.
        /// </remarks>
        public async Task<T> PerformAsync<T>(ApiRequest request, CancellationToken cancellationToken = default) where T : class
        {
            var serializer = Serializers.Resolve<T>(DataDirection.In);
            var requestMessage = await BuildRequest(request, serializer.ContentType).ConfigureAwait(false);

            return await PerformAsyncInternal<T>(requestMessage, serializer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a prebuilt <see cref="HttpRequestMessage"/>, deserializing the results into the specified type.
        /// </summary>
        public Task<T> PerformAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default) where T : class
        {
            return PerformAsyncInternal<T>(request, Serializers.Resolve<T>(DataDirection.In), cancellationToken);
        }

        private async Task<T> PerformAsyncInternal<T>(HttpRequestMessage request, ApiSerializer serializer, CancellationToken cancellationToken) where T : class
        {
            using (request)
            {
                var responseMessage = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                return await ValidateAndProcess<T>(responseMessage, serializer, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a GET request to the provided <see cref="url"/>, returning the raw <see cref="HttpResponseMessage"/>
        /// </summary>
        public Task<HttpResponseMessage> PerformAsync(string url, CancellationToken cancellationToken = default)
        {
            return PerformAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);
        }

        /// <summary>
        /// Performs a request, returning the raw <see cref="HttpResponseMessage"/> for the caller to process
        /// </summary>
        public async Task<HttpResponseMessage> PerformAsync(ApiRequest request, CancellationToken cancellationToken = default)
        {
            var requestMessage = await BuildRequest(request, "*/*").ConfigureAwait(false);
            return await PerformAsync(requestMessage, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a prebuilt <see cref="HttpRequestMessage"/>, returning the raw <see cref="HttpResponseMessage"/>
        /// </summary>
        public async Task<HttpResponseMessage> PerformAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
        {
            using (requestMessage)
            {
                return await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs a request, writing the response to the specified <see cref="Stream"/>
        /// </summary>
        /// <remarks>
        /// This method does *not* seek or modify the position of the stream, nor will it dispose of the <see cref="destination"/> stream.
        /// </remarks>
        /// <param name="request">The <see cref="ApiRequest"/> to make</param>
        /// <param name="destination">A <see cref="Stream"/> to write to.</param>
        /// <param name="progress">(Optional) progress callback</param>
        /// <param name="truncate">(Optional) whether to truncate the destination stream, if content is written. Defaults to true</param>
        /// <param name="safe">(Optional) whether to copy to a temporary buffer before writing to destination. When enabled provides greater redundancy from network failure. Defaults to false</param>
        /// <param name="cancellationToken">(Optional) cancellation request</param>
        public async Task<HttpStatusCode> PerformDownload(ApiRequest request, Stream destination, IProgress<(long, long?)>? progress = null, bool truncate = true, bool safe = false, CancellationToken cancellationToken = default)
        {
            using var requestMessage = await BuildRequest(request, "*/*").ConfigureAwait(false);
            return await PerformDownload(requestMessage, destination, progress, truncate, safe, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a request, writing the response to the specified <see cref="Stream"/>
        /// </summary>
        /// <remarks>
        /// This method does *not* seek or modify the position of the stream, nor will it dispose of the <see cref="destination"/> stream.
        /// </remarks>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to make</param>
        /// <param name="destination">A <see cref="Stream"/> to write to.</param>
        /// <param name="progress">(Optional) progress callback</param>
        /// <param name="truncate">(Optional) whether to truncate the destination stream, if content is written. Defaults to true</param>
        /// <param name="safe">(Optional) whether to copy to a temporary buffer before writing to destination. When enabled provides greater redundancy from network failure. Defaults to false</param>
        /// <param name="cancellationToken">(Optional) cancellation request</param>
        public async Task<HttpStatusCode> PerformDownload(HttpRequestMessage request, Stream destination, IProgress<(long, long?)>? progress = null, bool truncate = true, bool safe = false, CancellationToken cancellationToken = default)
        {
            using var responseMessage = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                return responseMessage.StatusCode;
            }

            if (!destination.CanWrite)
            {
                throw new ArgumentException("Destination Stream must be writable.", nameof(destination));
            }

            if (!destination.CanSeek && truncate)
            {
                throw new ArgumentException("Destination Stream must be seekable to use truncate.", nameof(destination));
            }

            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            var totalLength = responseMessage.Content.Headers.ContentLength;
            var destinationStream = safe switch
            {
                // when less than 80kb is copied, use a memory stream
                true when totalLength < 80000 => new MemoryStream(),

                // file stream for unknown or large files
                true => new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose),

                // write directly when not using safe mode
                _ => destination
            };

            try
            {
                int read;

                var loopCount = 0;
                var totalRead = 0L;

                // 0.5% of the total length or 250kb progress increments
                // get the number of read/write cycles to do before reporting progress
                var copies = (int)Math.Ceiling((totalLength / 200 ?? 2.5e+5) / buffer.Length);

                void UpdateProgress()
                {
                    totalRead += read;

                    if (++loopCount % copies != 0)
                    {
                        return;
                    }

                    progress?.Report((totalRead, totalLength));
                    loopCount = 0;
                }

#if NETSTANDARD2_0
                using var stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
                    UpdateProgress();
                }
#else
                using var stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var memory = buffer.AsMemory();

                while ((read = await stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await destinationStream.WriteAsync(memory[..read], cancellationToken).ConfigureAwait(false);
                    UpdateProgress();
                }
#endif

                // flush stream contents before truncating or copying
                await destinationStream.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (safe)
                {
                    // safe mode: copy temp file contents to destination
                    destinationStream.Seek(0, SeekOrigin.Begin);

#if NETSTANDARD2_0
                    await destinationStream.CopyToAsync(destination).ConfigureAwait(false);
#else
                    await destinationStream.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
#endif
                }

                // perform final truncate (if needed)
                if (truncate && destination.Length > destination.Position)
                {
                    destination.SetLength(destination.Position);
                }

                return HttpStatusCode.OK;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);

                if (safe)
                {
#if NETSTANDARD2_0
                    destinationStream.Dispose();
#else
                    await destinationStream.DisposeAsync().ConfigureAwait(false);
#endif
                }
            }
        }

        /// <summary>
        /// Overridable method used to control creation of a <see cref="HttpMessageHandler"/> used by the internal HTTP client.
        /// </summary>
        /// <remarks>
        /// This is designed to be used by libraries requiring overall control of handlers (i.e. wrap the user-selected handler to provide additional functionality)
        /// </remarks>
        protected virtual HttpMessageHandler CreateHandler() => Handler?.Invoke() ?? CreateDefaultHandler();

        /// <summary>
        /// Overridable method used to control creation of a <see cref="HttpClient"/> used by the internal HTTP client.
        /// </summary>
        protected virtual HttpClient CreateClient()
        {
            var client = new HttpClient(CreateHandler(), true);

#if !NETSTANDARD2_0
            // on newer platforms, enable HTTP/2 (and HTTP/3)
            client.DefaultRequestVersion = HttpVersion.Version11;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
#endif

            client.BaseAddress = _baseAddress;
            return client;
        }

        /// <summary>
        /// Overridable handler for validating and processing a <see cref="HttpResponseMessage"/>
        /// </summary>
        protected virtual async Task<T> ValidateAndProcess<T>(HttpResponseMessage response, ApiSerializer serializer, CancellationToken cancellationToken) where T : class
        {
            using (response)
            {
                response.EnsureSuccessStatusCode();

#if NETSTANDARD2_0
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif

                if (serializer is IAsyncSerializer asyncSerializer)
                {
                    return await asyncSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
                }

                return serializer.Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// Overridable method used to build a <see cref="HttpRequestMessage"/> from an <see cref="ApiRequest"/>
        /// </summary>
        /// <param name="request">The request to build a <see cref="HttpRequestMessage"/> for</param>
        /// <param name="expectedContentType">The Content-Type expected to be returned</param>
        /// <returns>The <see cref="HttpRequestMessage"/> to send</returns>
        protected virtual async ValueTask<HttpRequestMessage> BuildRequest(ApiRequest request, string expectedContentType)
        {
            switch (request)
            {
                case IRequestExecutingCallback callback:
                    callback.OnRequestExecuting(this);
                    break;

                case IAsyncRequestExecutingCallback asyncCallback:
                    await asyncCallback.OnRequestExecuting(this);
                    break;
            }

            var requestMessage = (request as IRequestBuilder)?.BuildRequest(Serializers) ?? ReflectionRequestMessageBuilder.CreateHttpRequestMessage(request, Serializers);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(expectedContentType));

            return requestMessage;
        }

        public static HttpMessageHandler CreateDefaultHandler()
        {
#if !NETSTANDARD2_0
            if (SocketsHttpHandler.IsSupported)
            {
                return new SocketsHttpHandler
                {
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.All,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10)
                };
            }
#endif
            return new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
        }
    }
}
