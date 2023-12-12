// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Serializers;

namespace DragonFruit.Data
{
    /// <summary>
    /// The <see cref="ApiClient"/> responsible for building, submitting and processing HTTP requests
    /// </summary>
    public class ApiClient
    {
        private HttpClient _client;

        public ApiClient(ApiSerializer serializer)
        {
            Serializers = new SerializerResolver(serializer);
        }

        ~ApiClient()
        {
            _client?.Dispose();
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
        /// The <see cref="SerializerResolver"/> instance used to resolve serializers for requests.
        /// Caches and reused serializers where possible.
        /// </summary>
        public SerializerResolver Serializers { get; }

        /// <summary>
        /// User-Controlled method to create a <see cref="HttpClientHandler"/>
        /// </summary>
        public Func<HttpClientHandler> Handler { get; set; }

        /// <summary>
        /// Gets the header container for the underlying <see cref="HttpClient"/>
        /// </summary>
        public HttpRequestHeaders Headers => Client.DefaultRequestHeaders;

        /// <summary>
        /// Gets the <see cref="HttpClient"/> used across all requests.
        /// </summary>
        protected HttpClient Client => _client ??= CreateClient();

        /// <summary>
        /// Overridable method used to control creation of a <see cref="HttpMessageHandler"/> used by the internal HTTP client.
        /// </summary>
        /// <remarks>
        /// This is designed to be used by libraries requiring overall control of handlers (i.e. wrap the user-selected handler to provide additional functionality)
        /// </remarks>
        protected virtual HttpMessageHandler CreateHandler() => Handler?.Invoke() ?? CreateDefaultHandler();

        /// <summary>
        /// Performs a request, deserializing the results into the specified type.
        /// </summary>
        /// <remarks>
        /// If source generation is enabled, the source generated method will be used to build the request, otherwise a legacy reflection handler will build the request.
        /// </remarks>
        public async Task<T> PerformAsync<T>(ApiRequest request, CancellationToken cancellationToken = default) where T : class
        {
            using var requestMessage = await BuildRequest(request, Serializers.Resolve<T>(DataDirection.In).ContentType).ConfigureAwait(false);
            using var responseMessage = await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            return await ValidateAndProcess<T>(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a request, returning the raw <see cref="HttpResponseMessage"/> for the caller to process
        /// </summary>
        public async Task<HttpResponseMessage> PerformAsync(ApiRequest request, CancellationToken cancellationToken = default)
        {
            using var requestMessage = await BuildRequest(request, "*/*").ConfigureAwait(false);
            return await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }

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

            return client;
        }

        /// <summary>
        /// Overridable handler for validating and processing a <see cref="HttpResponseMessage"/>
        /// </summary>
        protected virtual async Task<T> ValidateAndProcess<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : class
        {
            response.EnsureSuccessStatusCode();

#if NETSTANDARD2_0
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif

            var serializer = Serializers.Resolve<T>(DataDirection.In);

            if (serializer is IAsyncSerializer asyncSerializer)
            {
                return await asyncSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
            }

            return serializer.Deserialize<T>(stream);
        }

        /// <summary>
        /// Overridable method used to build a <see cref="HttpRequestMessage"/> from an <see cref="ApiRequest"/>
        /// </summary>
        /// <param name="request">The request to build a <see cref="HttpRequestMessage"/> for</param>
        /// <param name="expectedContentType">The Content-Type expected to be returned</param>
        /// <returns>The <see cref="HttpRequestMessage"/> to send</returns>
        protected virtual async ValueTask<HttpRequestMessage> BuildRequest(ApiRequest request, string expectedContentType)
        {
            if (request is IRequestExecutingCallback callback)
            {
                callback.OnRequestExecuting(this);
            }

            if (request is IAsyncRequestExecutingCallback asyncCallback)
            {
                await asyncCallback.OnRequestExecuting(this);
            }

            var requestMessage = request is IRequestBuilder rb
                ? rb.BuildRequest(Serializers)
                : ReflectionRequestMessageBuilder.CreateHttpRequestMessage(request, this);

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(expectedContentType));
            return requestMessage;
        }

        private HttpMessageHandler CreateDefaultHandler()
        {
#if NETSTANDARD2_0
            return new HttpClientHandler
            {
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
#else
            return new SocketsHttpHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(10)
            };
#endif
        }
    }

    /// <summary>
    /// Represents a strongly-typed serializer version of <see cref="ApiClient"/>
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ApiSerializer"/></typeparam>
    public class ApiClient<T> : ApiClient where T : ApiSerializer, new()
    {
        public ApiClient()
            : base(Activator.CreateInstance<T>())
        {
        }
    }
}
