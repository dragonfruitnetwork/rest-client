// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using DragonFruit.Common.Data.Serializers;

namespace DragonFruit.Common.Data
{
    /// <summary>
    /// <see cref="HttpClient"/>-related data
    /// </summary>
    public class ApiClient
    {
        /// <summary>
        /// The User-Agent string sent as a header
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Any additional headers to be sent
        /// </summary>
        public List<KeyValuePair<string, string>> CustomHeaders { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// The Authorization header
        /// </summary>
        public string Authorization { get; set; } = null;

        /// <summary>
        /// Optional <see cref="HttpClient"/> settings sent by the <see cref="HttpClientHandler"/>
        /// </summary>
        public HttpClientHandler Handler { get; set; } = null;

        /// <summary>
        /// Method for getting data
        /// </summary>
        public ISerializer Serializer { get; set; } = new ApiJsonSerializer();

        ///Hashes to determine whether we replace the <see cref="HttpClient" />
        private string _lastClientHash = string.Empty;
        public string ClientHash => $"{HashCodeOf(UserAgent)}.{HashCodeOf(CustomHeaders)}.{HashCodeOf(Handler)}.{HashCodeOf(Authorization)}";
        /// end hashes
        
        ///Clients and thread locks
        private bool _clientAdjustmentInProgress;
        private HttpClient Client { get; set; }
        ///end clients

        /// <summary>
        /// Perform a web request with an <see cref="ApiRequest"/>
        /// </summary>
        public T Perform<T>(ApiRequest requestData) where T : class
        {
            while (_clientAdjustmentInProgress)
                Thread.Sleep(200);

            if (!_lastClientHash.Equals(ClientHash))
            {
                _clientAdjustmentInProgress = true;

                //cleanup from old attempts
                Client?.Dispose();
                Client = Handler != null ? new HttpClient(Handler, true) : new HttpClient();
                var hasAuthData = !string.IsNullOrEmpty(Authorization);

                if (requestData.RequireAuth && !hasAuthData)
                    //todo custom exceptions
                    throw new Exception("Authorization data expected, but not found");

                if (hasAuthData)
                    Client.DefaultRequestHeaders.Add("Authorization", Authorization);

                if (!string.IsNullOrEmpty(UserAgent))
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                if (!string.IsNullOrEmpty(requestData.AcceptedContent))
                    Client.DefaultRequestHeaders.Accept.ParseAdd(requestData.AcceptedContent);

                foreach (var header in CustomHeaders)
                    Client.DefaultRequestHeaders.Add(header.Key, header.Value);

                _lastClientHash = ClientHash;
                _clientAdjustmentInProgress = false;
            }

            //method specific modes and returns
            switch (requestData.Method)
            {
                case Methods.Get:
                    return Serializer.Deserialize<T>(Client.GetStreamAsync(requestData.Url));

                case Methods.PostForm:
                    return Serializer.Deserialize<T>(Client.PostAsync(requestData.Url, requestData.FormContent).Result.Content.ReadAsStreamAsync());

                case Methods.PostString:
                    return Serializer.Deserialize<T>(Client.PostAsync(requestData.Url, Serializer.Serialize(requestData)).Result.Content.ReadAsStreamAsync());

                case Methods.PutForm:
                    return Serializer.Deserialize<T>(Client.PutAsync(requestData.Url, requestData.FormContent).Result.Content.ReadAsStreamAsync());

                case Methods.PutString:
                    return Serializer.Deserialize<T>(Client.PutAsync(requestData.Url, Serializer.Serialize(requestData)).Result.Content.ReadAsStreamAsync());

                default:
                    throw new NotImplementedException();
            }
        }

        private string HashCodeOf(object data) => data == null ? "!" : data.GetHashCode().ToString();
    }
}