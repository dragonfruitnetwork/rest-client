// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Common.Data.Exceptions;
using DragonFruit.Common.Data.Extensions;
using DragonFruit.Common.Data.Headers;
using DragonFruit.Common.Data.Serializers;

namespace DragonFruit.Common.Data
{
    /// <summary>
    /// <see cref="HttpClient"/>-related data
    /// </summary>
    public class ApiClient
    {
        #region Constructors

        public ApiClient()
        {
            Serializer = new ApiJsonSerializer(CultureInfo.InvariantCulture);
        }

        public ApiClient(CultureInfo culture)
        {
            Serializer = new ApiJsonSerializer(culture);
        }

        public ApiClient(ISerializer serialiser)
        {
            Serializer = serialiser;
        }

        ~ApiClient()
        {
            Client?.Dispose();
            Handler?.Dispose();
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
        public HeaderCollection Headers { get; } = new HeaderCollection();

        /// <summary>
        /// Optional <see cref="HttpMessageHandler"/> to be consumed by the <see cref="HttpClient"/>
        /// </summary>
        /// <remarks>
        /// The old <see cref="HttpMessageHandler"/> will be disposed on setting a new one.
        /// </remarks>
        protected HttpMessageHandler Handler { get; set; }

        /// <summary>
        /// The <see cref="ISerializer"/> to use when encoding/decoding request and response streams.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="ApiJsonSerializer"/>
        /// </remarks>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// <see cref="HttpClient"/> used by these requests. This is used by the library and as such, should **not** be disposed in any way
        /// </summary>
        protected HttpClient Client { get; private set; }

        /// <summary>
        /// Time, in milliseconds to wait to modify a <see cref="HttpClient"/> before failing the request
        /// </summary>
        protected virtual int AdjustmentTimeout => 200;

        #endregion

        #region Clients, Hashes and Locks

        private bool _clientAdjustmentInProgress;
        private string _lastHandlerHash = string.Empty;
        private string _lastHash = string.Empty;

        private readonly object _clientAdjustmentLock = new object();
        private long _currentRequests;

        private HttpMessageHandler _handler;

        protected virtual string ClientHash => $"{Handler.ItemHashCode()}";

        #endregion

        #region Default Creations

        /// <summary>
        /// Checks the current <see cref="HttpClient"/> and replaces it if headers or <see cref="Handler"/> has been modified
        /// </summary>
        protected HttpClient GetClient()
        {
            // if we're waiting, then don't cause a crash from the monitor below or from getting the wrong client - just wait.
            while (_clientAdjustmentInProgress)
            {
                Thread.Sleep(AdjustmentTimeout / 2);
            }

            //if there's no edits return the current client (perform the check once instead of a potential twice)
            var changeHeaders = Headers.ChangesAvailable;

            if (_lastHash == ClientHash && !changeHeaders)
            {
                return Client;
            }

            try
            {
                _clientAdjustmentInProgress = true;

                //lock for modification
                if (!Monitor.TryEnter(_clientAdjustmentLock, AdjustmentTimeout))
                {
                    throw new TimeoutException($"The {nameof(ApiClient)} is being overloaded with reconstruction requests. Consider creating a separate {nameof(ApiClient)} and delegating clients to specific types of requests");
                }

                //wait for all ongoing requests to end
                while (_currentRequests > 0)
                {
                    Thread.Sleep(AdjustmentTimeout / 2);
                }

                var handlerHash = Handler.ItemHashCode();
                var resetClient = handlerHash != _lastHandlerHash;

                // only reset the client if the handler has changed.
                if (resetClient)
                {
                    Client?.Dispose();
                    _handler?.Dispose();

                    _handler = Handler;
                    _lastHandlerHash = handlerHash;

                    Client = Handler != null ? new HttpClient(_handler, false) : new HttpClient();
                }

                // reset the headers if any have changed (or the client has been reinitialised)
                if (changeHeaders || resetClient)
                {
                    // Clear and apply new headers
                    Client.DefaultRequestHeaders.Clear();
                    Headers.ProcessAndApplyTo(Client);

                    SetupClient(Client, resetClient);
                }

                _lastHash = ClientHash;
                return Client;
            }
            finally
            {
                _clientAdjustmentInProgress = false;
                Monitor.Exit(_clientAdjustmentLock);
            }
        }

        #endregion

        #region Empty Overrides (Inherited)

        /// <summary>
        /// Overridable method to customise the <see cref="HttpClient"/>.
        ///
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
        public virtual HttpResponseMessage Perform(ApiRequest requestData)
        {
            ValidateRequest(requestData);
            return Perform(requestData.Build(this));
        }

        /// <summary>
        /// Perform an <see cref="ApiRequest"/> with a specified return type.
        /// </summary>
        public virtual T Perform<T>(ApiRequest requestData) where T : class
        {
            ValidateRequest(requestData);
            return Perform<T>(requestData.Build(this));
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/>
        /// </summary>
        public virtual HttpResponseMessage Perform(HttpRequestMessage request)
        {
            return InternalPerform(request, response => response, false);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/> and deserialize the result to the specified type
        /// </summary>
        public virtual T Perform<T>(HttpRequestMessage request) where T : class
        {
            return InternalPerform(request, response => ValidateAndProcess<T>(response, request), true);
        }

        /// <summary>
        /// Download a file with an <see cref="ApiRequest"/>. Bypasses <see cref="ValidateAndProcess{T}"/>
        /// </summary>
        public virtual void Perform(ApiFileRequest requestData)
        {
            //check request data is valid
            ValidateRequest(requestData);

            if (string.IsNullOrWhiteSpace(requestData.Destination))
            {
                throw new NullRequestException();
            }

            HttpResponseMessage CopyProcess(HttpResponseMessage response)
            {
                //validate
                response.EnsureSuccessStatusCode();

                //copy result to file
                using (var stream = File.Open(requestData.Destination, requestData.FileCreationMode))
                using (var networkStream = response.Content.ReadAsStreamAsync().Result)
                {
                    networkStream.CopyTo(stream);
                }

                return response; //we're not using this so return anything...
            }

            _ = InternalPerform(requestData.Build(this), CopyProcess, true);
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
        protected T InternalPerform<T>(HttpRequestMessage request, Func<HttpResponseMessage, T> processResult, bool disposeResponse)
        {
            //get client and request (disposables)
            var client = GetClient();
            Interlocked.Increment(ref _currentRequests);

            //post-modification
            SetupRequest(request);

            //send request
            var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            try
            {
                //all possible exceptions from client.SendAsync() will be released here
                return processResult.Invoke(response.Result);
            }
            finally
            {
                //un-bump reqs
                Interlocked.Decrement(ref _currentRequests);

                //dispose
                if (disposeResponse)
                {
                    response?.Result?.Dispose();
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
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response was unsuccessful ({response.StatusCode})");
            }

            using var stream = response.Content.ReadAsStreamAsync();
            return Serializer.Deserialize<T>(stream);
        }

        /// <summary>
        /// An overridable method for validating the request against the current <see cref="ApiClient"/>
        /// </summary>
        /// <param name="requestData">The request to validate</param>
        /// <exception cref="NullRequestException">The request can't be performed due to a poorly-formed url</exception>
        /// <exception cref="ClientValidationException">The client can't be used because there is no auth url.</exception>
        protected virtual void ValidateRequest(ApiRequest requestData)
        {
            // note request path is validated on build
            if (requestData.RequireAuth && string.IsNullOrEmpty(Authorization))
            {
                // check if we have a custom headerset in the request
                if (!requestData.Headers.IsValueCreated || !requestData.Headers.Value.ContainsKey("Authorization"))
                {
                    throw new ClientValidationException("Authorization header was expected, but not found (in request or client)");
                }
            }
        }
    }
}
