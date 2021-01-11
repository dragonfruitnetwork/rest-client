// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Common.Data.Exceptions;
using DragonFruit.Common.Data.Headers;
using DragonFruit.Common.Data.Serializers;

namespace DragonFruit.Common.Data
{
    /// <summary>
    /// Managed wrapper for a <see cref="HttpClient"/> allowing easy header access, handler, serializing/deserializing and memory management
    /// </summary>
    public class ApiClient
    {
        #region Constructors

        /// <summary>
        /// Initialises a new <see cref="ApiClient"/> using an <see cref="ApiJsonSerializer"/> using the <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public ApiClient()
            : this(new ApiJsonSerializer(CultureInfo.InvariantCulture))
        {
        }

        /// <summary>
        /// Initialises a new <see cref="ApiClient"/> using an <see cref="ApiJsonSerializer"/> using a custom <see cref="CultureInfo"/>
        /// </summary>
        public ApiClient(CultureInfo culture)
            : this(new ApiJsonSerializer(culture))
        {
        }

        /// <summary>
        /// Initialises a new <see cref="ApiClient"/> using a user-set <see cref="ISerializer"/>
        /// </summary>
        public ApiClient(ISerializer serializer)
        {
            Serializer = serializer;
            Headers = new HeaderCollection(this);

            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            RequestClientReset(true);
        }

        ~ApiClient()
        {
            _lock?.Dispose();

            Client?.Dispose();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The User-Agent header to pass in all requests
        /// </summary>
        public string UserAgent
        {
            get => Headers["User-Agent"];
            set => Headers["User-Agent"] = value;
        }

        /// <summary>
        /// The Authorization header value
        /// </summary>
        public string Authorization
        {
            get => Headers["Authorization"];
            set => Headers["Authorization"] = value;
        }

        /// <summary>
        /// Headers to be sent with the requests
        /// </summary>
        public HeaderCollection Headers { get; }

        /// <summary>
        /// Optional <see cref="HttpMessageHandler"/> factory to be consumed by the <see cref="HttpClient"/>
        /// </summary>
        /// <remarks>
        /// This must create a new handler each time as they are disposed alongside the client
        /// </remarks>
        public Func<HttpMessageHandler> Handler
        {
            get => _handler;
            set
            {
                _handler = value;
                RequestClientReset(true);
            }
        }

        /// <summary>
        /// The <see cref="ISerializer"/> to use when encoding/decoding request and response streams.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="ApiJsonSerializer"/>
        /// </remarks>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// <see cref="HttpClient"/> used by these requests.
        /// This is used by the library and as such, should **not** be disposed in any way
        /// </summary>
        protected HttpClient Client { get; private set; }

        #endregion

        #region Private Vars

        private readonly ReaderWriterLockSlim _lock;
        private long _clientAdjustmentRequestSignal;
        private Func<HttpMessageHandler> _handler;

        #endregion

        #region Factories

        /// <summary>
        /// Checks the current <see cref="HttpClient"/> and replaces it if headers or <see cref="Handler"/> has been modified
        /// </summary>
        protected HttpClient GetClient()
        {
            // return current client if there are no changes
            var resetLevel = Interlocked.Read(ref _clientAdjustmentRequestSignal);

            if (resetLevel > 0)
            {
                // block all reads and let all current requests finish
                _lock.EnterWriteLock();

                try
                {
                    // only reset the client if the handler has changed (signal = 2)
                    var resetClient = resetLevel == 2;

                    if (resetClient)
                    {
                        var handler = CreateHandler();

                        Client?.Dispose();
                        Client = handler != null ? new HttpClient(handler, true) : new HttpClient();
                    }

                    // apply new headers
                    Headers.ApplyTo(Client);

                    // allow the consumer to change the client
                    SetupClient(Client, resetClient);

                    // reset the state
                    Interlocked.Exchange(ref _clientAdjustmentRequestSignal, 0);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            _lock.EnterReadLock();
            return Client;
        }

        #endregion

        #region Empty Overrides (Inherited)

        /// <summary>
        /// Overridable method for creating a <see cref="HttpMessageHandler"/> to use with the <see cref="HttpClient"/>
        /// </summary>
        protected virtual HttpMessageHandler CreateHandler() => Handler?.Invoke();

        /// <summary>
        /// Overridable method to customise the <see cref="HttpClient"/>.
        /// <para>
        /// Custom headers can be included here, but should be done in the <see cref="Headers"/> dictionary.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This is called when the client or it's headers are reset.
        /// The <see cref="clientReset"/> is set to true to allow you to configure client settings (not headers) after creation.
        /// </remarks>
        /// <param name="client">The <see cref="HttpClient"/> to modify</param>
        /// <param name="clientReset">Whether the client were reset/disposed</param>
        protected virtual void SetupClient(HttpClient client, bool clientReset)
        {
        }

        /// <summary>
        /// When overridden, this can be used to alter all <see cref="HttpRequestMessage"/> created.
        /// </summary>
        protected virtual void SetupRequest(HttpRequestMessage request)
        {
        }

        #endregion

        /// <summary>
        /// Perform a <see cref="ApiRequest"/> that returns the response message. The <see cref="HttpResponseMessage"/> returned cannot be used for reading data, as the underlying <see cref="Task"/> will be disposed.
        /// </summary>
        public virtual HttpResponseMessage Perform(ApiRequest requestData, CancellationToken token = default)
        {
            ValidateRequest(requestData);
            return Perform(requestData.Build(this), token);
        }

        /// <summary>
        /// Perform an <see cref="ApiRequest"/> with a specified return type.
        /// </summary>
        public virtual T Perform<T>(ApiRequest requestData, CancellationToken token = default) where T : class
        {
            ValidateRequest(requestData);
            return Perform<T>(requestData.Build(this), token);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/>
        /// </summary>
        public virtual HttpResponseMessage Perform(HttpRequestMessage request, CancellationToken token = default)
        {
            return InternalPerform(request, response => response, false, token);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/> and deserialize the result to the specified type
        /// </summary>
        public virtual T Perform<T>(HttpRequestMessage request, CancellationToken token = default) where T : class
        {
            return InternalPerform(request, response => ValidateAndProcess<T>(response, request), true, token);
        }

        /// <summary>
        /// Download a file with an <see cref="ApiRequest"/>.
        /// Bypasses <see cref="ValidateAndProcess{T}"/>
        /// </summary>
        public virtual void Perform(ApiFileRequest request, Action<long, long?> progressUpdated = null, CancellationToken token = default)
        {
            //check request data is valid
            ValidateRequest(request);

            if (string.IsNullOrWhiteSpace(request.Destination))
            {
                throw new NullRequestException();
            }

            HttpResponseMessage CopyProcess(HttpResponseMessage response)
            {
                // validate
                response.EnsureSuccessStatusCode();

                // create a new filestream and copy all data into
                using var stream = File.Open(request.Destination, request.FileCreationMode);
#if NET5_0
                using var networkStream = response.Content.ReadAsStreamAsync(token).Result;
#else
                using var networkStream = response.Content.ReadAsStreamAsync().Result;
#endif
                // create a buffer for progress reporting
                var buffer = new byte[request.BufferSize];
                int count;
                uint iterations = 0;

                while ((count = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    iterations++;
                    stream.Write(buffer, 0, count);

                    if (iterations % 25 == 0)
                    {
                        progressUpdated?.Invoke(stream.Length, response.Content.Headers.ContentLength);
                    }
                }

                // flush and return
                stream.Flush();

                // we're not using this so return anything...
                return response;
            }

            _ = InternalPerform(request.Build(this), CopyProcess, true, token);
        }

        /// <summary>
        /// Internal procedure for performing a web-request
        /// </summary>
        /// <remarks>
        /// While the consumer has the option to prevent disposal of the <see cref="HttpResponseMessage"/> produced,
        /// the <see cref="HttpRequestMessage"/> passed is always disposed at the end of the request.
        /// </remarks>
        /// <param name="request">The request to perform</param>
        /// <param name="processResult"><see cref="Func{T,TResult}"/> to process the <see cref="HttpResponseMessage"/></param>
        /// <param name="disposeResponse">Whether to dispose of the <see cref="HttpResponseMessage"/> produced after <see cref="processResult"/> has been invoked.</param>
        protected T InternalPerform<T>(HttpRequestMessage request, Func<HttpResponseMessage, T> processResult, bool disposeResponse, CancellationToken token = default)
        {
            var client = GetClient();

            // post-modification
            SetupRequest(request);
            HttpResponseMessage response = null;

            try
            {
                token.ThrowIfCancellationRequested();

                // send request
#if NET5_0
                response = client.Send(request, HttpCompletionOption.ResponseHeadersRead, token);
#else
                response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).Result;
#endif

                // all possible exceptions from client.SendAsync() will be released here
                return processResult.Invoke(response);
            }
            finally
            {
                RequestFinished();

                // dispose
                if (disposeResponse)
                {
                    response?.Dispose();
                }

                request?.Dispose();
            }
        }

        /// <summary>
        /// Validates the <see cref="HttpResponseMessage"/> and uses the <see cref="Serializer"/> to deserialize data (if successful)
        /// </summary>
        protected virtual T ValidateAndProcess<T>(HttpResponseMessage response, HttpRequestMessage request) where T : class
        {
            response.EnsureSuccessStatusCode();

            using var stream = response.Content.ReadAsStreamAsync().Result;
            return Serializer.Deserialize<T>(stream);
        }

        /// <summary>
        /// An overridable method for validating the request against the current <see cref="ApiClient"/>
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <exception cref="NullRequestException">The request can't be performed due to a poorly-formed url</exception>
        /// <exception cref="ClientValidationException">The client can't be used because there is no auth url.</exception>
        protected virtual void ValidateRequest(ApiRequest request)
        {
            // note request path is validated on build
            if (request.RequireAuth && string.IsNullOrEmpty(Authorization))
            {
                // check if we have a custom headerset in the request
                if (!request.CustomHeaderCollectionCreated || !request.Headers.Any(x => x.Key.Equals("Authorization")))
                {
                    throw new ClientValidationException("Authorization header was expected, but not found (in request or client)");
                }
            }
        }

        /// <summary>
        /// Requests the client is reset on the next request
        /// </summary>
        /// <param name="fullReset">Whether to reset the <see cref="HttpClient"/> as well as the headers</param>
        public void RequestClientReset(bool fullReset)
        {
            if (fullReset)
            {
                Interlocked.Exchange(ref _clientAdjustmentRequestSignal, 2);
            }
            else
            {
                Interlocked.CompareExchange(ref _clientAdjustmentRequestSignal, 1, 0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RequestFinished() => _lock.ExitReadLock();
    }
}
