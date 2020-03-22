// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Common.Data.Exceptions;
using DragonFruit.Common.Data.Helpers;
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
            Serializer = new ApiJsonSerializer();
        }

        public ApiClient(CultureInfo culture)
        {
            Serializer = new ApiJsonSerializer(culture);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The User-Agent header value
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Additional headers to be sent with the requests
        /// </summary>
        public List<KeyValuePair<string, string>> CustomHeaders { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// The Authorization value
        /// </summary>
        public string Authorization { get; set; } = null;

        /// <summary>
        /// Optional <see cref="HttpClient"/> settings sent by the <see cref="HttpClientHandler"/>
        /// </summary>
        public HttpClientHandler Handler { get; set; } = null;

        /// <summary>
        /// Method for getting data
        /// </summary>
        public ISerializer Serializer { get; set; }

        #endregion

        /// <summary>
        /// Checksum that determines whether we replace the <see cref="HttpClient"/>
        /// </summary>
        protected virtual string ClientHash => $"{UserAgent.ItemHashCode()}.{CustomHeaders.ItemHashCode()}.{Handler.ItemHashCode()}.{Authorization.ItemHashCode()}";

        #region Clients, Hashes and Locks

        private bool _clientAdjustmentInProgress;
        private string _lastClientHash = string.Empty;
        private HttpClient _client;

        #endregion

        /// <summary>
        /// Checks the current <see cref="HttpClient"/> and replaces it if headers or <see cref="Handler"/> has been modified
        /// </summary>
        protected virtual HttpClient GetClient(ApiRequest requestData)
        {
            while (_clientAdjustmentInProgress)
                Thread.Sleep(200);

            if (!_lastClientHash.Equals(ClientHash))
            {
                _clientAdjustmentInProgress = true;

                //cleanup from old attempts
                _client?.Dispose();
                _client = Handler != null ? new HttpClient(Handler, true) : new HttpClient();
                var hasAuthData = !string.IsNullOrEmpty(Authorization);

                if (requestData.RequireAuth && !hasAuthData)
                    throw new ClientValidationException("Authorization data expected, but not found");

                if (hasAuthData)
                    _client.DefaultRequestHeaders.Add("Authorization", Authorization);

                if (!string.IsNullOrEmpty(UserAgent))
                    _client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                if (!string.IsNullOrEmpty(requestData.AcceptedContent))
                    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(requestData.AcceptedContent));

                foreach (var header in CustomHeaders)
                    _client.DefaultRequestHeaders.Add(header.Key, header.Value);

                SetupClient(_client);

                _lastClientHash = ClientHash;
                _clientAdjustmentInProgress = false;
            }

            return _client;
        }

        /// <summary>
        /// Add your own headers/settings to the <see cref="HttpClient"/> being created. Runs after the headers have been added
        /// </summary>
        /// <param name="client"></param>
        protected virtual void SetupClient(HttpClient client) { }

        /// <summary>
        /// Perform a web request with an <see cref="ApiRequest"/>
        /// </summary>
        public virtual T Perform<T>(ApiRequest requestData) where T : class
        {
            var client = GetClient(requestData);
            Task<HttpResponseMessage> response;

            //method specific request methods
            switch (requestData.Method)
            {
                case Methods.Get:
                    response = client.GetAsync(requestData.Url);
                    break;

                case Methods.PostForm:
                    response = client.PostAsync(requestData.Url, requestData.FormContent);
                    break;

                case Methods.PostString:
                    response = client.PostAsync(requestData.Url, Serializer.Serialize(requestData));
                    break;

                case Methods.PutForm:
                    response = client.PutAsync(requestData.Url, requestData.FormContent);
                    break;

                case Methods.PutString:
                    response = client.PutAsync(requestData.Url, Serializer.Serialize(requestData));
                    break;

                default:
                    throw new NotImplementedException();
            }

            return ValidateAndProcess<T>(response);
        }

        /// <summary>
        /// Validates the <see cref="HttpResponseMessage"/> and uses the <see cref="Serializer"/> to deserialize data (if successful)
        /// </summary>
        protected virtual T ValidateAndProcess<T>(Task<HttpResponseMessage> response) where T : class
        {
            if (!response.Result.IsSuccessStatusCode)
                throw new HttpRequestException($"Response was not successful ({response.Result.StatusCode})");

            return Serializer.Deserialize<T>(response.Result.Content.ReadAsStreamAsync());
        }
    }
}
