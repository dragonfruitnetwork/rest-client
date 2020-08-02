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
using DragonFruit.Common.Data.Serializers;
using DragonFruit.Common.Data.Utils;

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

        public ApiClient(ISerializer serializer)
        {
            Serializer = serializer;
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
        public string UserAgent { get; set; }

        /// <summary>
        /// Additional headers to be sent with the requests
        /// </summary>
        public HashableDictionary<string, string> CustomHeaders { get; set; } = new HashableDictionary<string, string>();

        /// <summary>
        /// The Authorization header
        /// </summary>
        public string Authorization { get; set; }

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
        protected ISerializer Serializer { get; set; }

        /// <summary>
        /// <see cref="HttpClient"/> used by these requests. This is used by the library and as such, should **not** be disposed in any way
        /// </summary>
        protected HttpClient Client { get; private set; }

        /// <summary>
        /// Time, in milliseconds to wait to modify a <see cref="HttpClient"/> before failing the request
        /// </summary>
        protected virtual int AdjustmentTimeout => 200;

        /// <summary>
        /// Last <see cref="ApiRequest"/> made for using with
        /// </summary>
        private ApiRequest CachedRequest { get; set; }

        #endregion

        #region Clients, Hashes and Locks

        private bool _clientAdjustmentInProgress;
        private string _lastHeaderHash = string.Empty;
        private string _lastHandlerHash = string.Empty;
        private string _lastHash = string.Empty;

        private readonly object _clientAdjustmentLock = new object();
        private long _currentRequests;

        private HttpMessageHandler _handler;

        protected virtual string ClientHash => $"{HeaderHash}.{Handler.ItemHashCode()}";
        private string HeaderHash => $"{UserAgent.ItemHashCode()}.{CustomHeaders.ItemHashCode()}.{Authorization.ItemHashCode()}";

        #endregion

        #region Default Creations

        /// <summary>
        /// Checks the current <see cref="HttpClient"/> and replaces it if headers or <see cref="Handler"/> has been modified
        /// </summary>
        protected virtual HttpClient GetClient()
        {
            while (_clientAdjustmentInProgress)
            {
                Thread.Sleep(AdjustmentTimeout / 2);
            }

            //if there's no edits return the current client
            if (_lastHash == ClientHash)
            {
                return Client;
            }

            try
            {
                _clientAdjustmentInProgress = true;

                //lock for modification
                if (!Monitor.TryEnter(_clientAdjustmentLock, AdjustmentTimeout))
                {
                    throw new TimeoutException(
                        $"The {nameof(ApiClient)} is being overloaded with reconstruction requests. Consider creating a separate {nameof(ApiClient)} and delegating clients to specific types of requests");
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
                var headerHash = HeaderHash;
                var resetHeaders = headerHash != _lastHeaderHash;

                if (resetHeaders || resetClient)
                {
                    Client.DefaultRequestHeaders.Clear();

                    // Authorization
                    if (!string.IsNullOrEmpty(Authorization))
                    {
                        Client.DefaultRequestHeaders.Add("Authorization", Authorization);
                    }

                    // User-Agent
                    if (!string.IsNullOrEmpty(UserAgent))
                    {
                        Client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                    }

                    // Custom Headers
                    foreach (var header in CustomHeaders)
                    {
                        Client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                    _lastHeaderHash = headerHash;

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
        /// Custom headers can be included here, but should be done in the <see cref="CustomHeaders"/> dictionary.
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

            // perform and return postProcess result
            return InternalPerform(requestData.GetRequest(Serializer), response => response, false);
        }

        /// <summary>
        /// Perform a pre-fabricated <see cref="HttpRequestMessage"/>
        /// </summary>
        public virtual HttpResponseMessage Perform(HttpRequestMessage request)
        {
            return InternalPerform(request, response => response, false);
        }

        /// <summary>
        /// Perform an <see cref="ApiRequest"/> with a specified return type.
        /// </summary>
        public virtual T Perform<T>(ApiRequest requestData) where T : class
        {
            ValidateRequest(requestData);
            var request = requestData.GetRequest(Serializer);

            return InternalPerform(request, response => ValidateAndProcess<T>(response, request), true);
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

            _ = InternalPerform(requestData.GetRequest(Serializer), CopyProcess, true);
        }

        /// <summary>
        /// Internal procedure for performing a web-request
        /// </summary>
        /// <param name="request">The request to perform</param>
        /// <param name="processResult"><see cref="Func{T,TResult}"/> to process the <see cref="HttpResponseMessage"/></param>
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

            return Serializer.Deserialize<T>(response.Content.ReadAsStreamAsync());
        }

        /// <summary>
        /// An overridable method for validating the request against the current <see cref="ApiClient"/>
        /// </summary>
        /// <param name="requestData">The request to validate</param>
        /// <exception cref="NullRequestException">The request can't be performed due to a poorly-formed url</exception>
        /// <exception cref="ClientValidationException">The client can't be used because there is no auth url.</exception>
        protected virtual void ValidateRequest(ApiRequest requestData)
        {
            //todo is there any benefit to trying to parse the url?
            if (string.IsNullOrWhiteSpace(requestData.Path))
            {
                throw new NullRequestException();
            }

            if (requestData.RequireAuth && (!requestData.Headers.IsValueCreated && string.IsNullOrEmpty(Authorization)))
            {
                throw new ClientValidationException("Authorization data expected, but not found");
            }
        }
    }
}
