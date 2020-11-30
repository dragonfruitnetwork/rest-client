// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Net.Http;
using DragonFruit.Common.Data.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Data.Services
{
    /// <summary>
    /// Basic web client methods for simpler tasks
    /// </summary>
    public static class WebServices
    {
        private static HttpClient _client;
        private static Func<HttpClient> _clientFactory;

        public static HttpClient Client => _client ??= ClientFactory.Invoke();

        /// <summary>
        /// <see cref="Func{TResult}"/> that creates the <see cref="HttpClient"/> to use
        /// </summary>
        public static Func<HttpClient> ClientFactory
        {
            get => _clientFactory ?? (() => new HttpClient());
            set => _clientFactory = value;
        }

        /// <summary>
        /// Disposes the current client and sets the <see cref="Client"/> property to null.
        /// </summary>
        /// <remarks>
        /// This will then force the <see cref="ClientFactory"/> to be invoked to get a new client on the next request
        /// </remarks>
        public static void ResetClient()
        {
            _client.Dispose();
            _client = null;
        }

        /// <summary>
        /// Download a JSON-based object with low memory consumption
        /// </summary>
        /// <typeparam name="T">Type to deserialize the result to</typeparam>
        /// <param name="uri">The Uri containing the resource</param>
        /// <returns>The specified tye <see cref="T" />, with the data converted</returns>
        public static T StreamObject<T>(string uri)
        {
            return StreamObject<T>(uri, ServiceUtils.DefaultSerializer);
        }

        /// <summary>
        /// Download a JSON-encoded object with low memory consumption
        /// </summary>
        /// <typeparam name="T">Type to deserialize the result to</typeparam>
        /// <param name="uri">The Uri containing the resource</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use when deserialising</param>
        /// <returns>The specified type <see cref="T" />, with the data converted</returns>
        public static T StreamObject<T>(string uri, JsonSerializer serializer)
        {
            using (var s = Client.GetStreamAsync(uri).Result)
            using (var sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(reader);
            }
        }

        /// <summary>
        /// Download a JSON-based object as a <see cref="JObject" /> with low memory consumption
        /// </summary>
        /// <param name="uri">The Uri containing the resource</param>
        /// <returns>JObject containing downloaded data</returns>
        public static JObject StreamObject(string uri)
        {
            return StreamObject<JObject>(uri);
        }

        /// <summary>
        /// Make a POST Request and get response as JObject
        /// </summary>
        /// <param name="uri">Intended Target</param>
        /// <param name="content">Http content data</param>
        /// <returns>JObject containing response data</returns>
        public static JObject PostData(string uri, HttpContent content)
        {
            return PostData<JObject>(uri, content, ServiceUtils.DefaultSerializer);
        }

        /// <summary>
        /// Make a POST Request and get response as specified type
        /// </summary>
        /// <param name="uri">Intended Target</param>
        /// <param name="content">HttpContent Data</param>
        /// <returns>Type containing response data</returns>
        public static T PostData<T>(string uri, HttpContent content)
        {
            return PostData<T>(uri, content, ServiceUtils.DefaultSerializer);
        }

        /// <summary>
        /// Make a POST Request and get response as specified type
        /// </summary>
        /// <param name="uri">Intended Target</param>
        /// <param name="content">HttpContent Data</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use when deserializing</param>
        /// <returns>Type containing response data</returns>
        public static T PostData<T>(string uri, HttpContent content, JsonSerializer serializer)
        {
            using (var s = Client.PostAsync(uri, content).Result.Content.ReadAsStreamAsync().Result)
            using (var sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
