// DragonFruit.Common Copyright 2019 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Storage.Web
{
    public static class WebServices
    {
        /// <summary>
        ///     Create a <see cref="HttpClient" /> with a specific UserAgent header
        /// </summary>
        /// <param name="userAgent">string of the UserAgent to be set</param>
        /// <returns><see cref="HttpClient" /> with a set UserAgent</returns>
        public static HttpClient GetClient(string userAgent = "")
        {
            var client = new HttpClient();

            if (!string.IsNullOrWhiteSpace(userAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            return client;
        }

        /// <summary>
        ///     Download a JSON-based object with low memory consumption and custom <see cref="HttpClient" />
        /// </summary>
        /// <typeparam name="T">Type to deserialize the result to</typeparam>
        /// <param name="uri">The Uri containing the resource</param>
        /// <param name="client"><see cref="HttpClient" /> to use when downloading</param>
        /// <returns>The specified tye <see cref="T" />, with the data converted</returns>
        public static T StreamObject<T>(string uri, HttpClient client)
        {
            using (Stream s = client.GetStreamAsync(uri).Result)
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
                return Shared.Newtonsoft.Serializer.Deserialize<T>(reader);
        }


        /// <summary>
        ///     Download a JSON-based object with low memory consumption
        /// </summary>
        /// <typeparam name="T">Type to deserialize the result to</typeparam>
        /// <param name="uri">The Uri containing the resource</param>
        /// <returns>The specified tye <see cref="T" />, with the data converted</returns>
        public static T StreamObject<T>(string uri)
        {
            using (HttpClient client = new HttpClient())
                return StreamObject<T>(uri, client);
        }

        /// <summary>
        ///     Download a JSON-based object as a <see cref="JObject" /> with low memory consumption and custom
        ///     <see cref="HttpClient" />
        /// </summary>
        /// <param name="uri">The Uri containing the resource</param>
        /// <param name="client"><see cref="HttpClient" /> to use when downloading</param>
        /// <returns>JObject containing downloaded data</returns>
        public static JObject StreamObject(string uri, HttpClient client)
        {
            using (Stream s = client.GetStreamAsync(uri).Result)
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
                return JObject.Load(reader);
        }


        /// <summary>
        ///     Download a JSON-based object as a <see cref="JObject" /> with low memory consumption
        /// </summary>
        /// <param name="uri">The Uri containing the resource</param>
        /// <returns>JObject containing downloaded data</returns>
        public static JObject StreamObject(string uri)
        {
            using (HttpClient client = new HttpClient())
                return StreamObject(uri, client);
        }

        /// <summary>
        /// Make a POST Request and get response as JObject
        /// </summary>
        /// <param name="uri">Intended Target</param>
        /// <param name="content">HttpContent Data</param>
        /// <param name="client">HttpClient to use</param>
        /// <returns>JObject containing response data</returns>
        public static JObject PostData(string uri, HttpContent content, HttpClient client)
        {
            using (Stream s = client.PostAsync(uri, content).Result.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
                return JObject.Load(reader);
        }

        /// <summary>
        /// Make a POST Request and get response as JObject
        /// </summary>
        /// <param name="uri">Intended Target</param>
        /// <param name="content">HttpContent Data</param>
        /// <returns>JObject containing response data</returns>
        public static JObject PostData(string uri, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
                return PostData(uri, content, client);
        }

        /// <summary>
        /// Make a POST Request and get response as specified type
        /// </summary>
        /// <param name="uri">Intended Target</param>
        /// <param name="content">HttpContent Data</param>
        /// <param name="client">HttpClient to use</param>
        /// <returns>Type containing response data</returns>
        public static T PostData<T>(string uri, HttpContent content, HttpClient client)
        {
            using (Stream s = client.PostAsync(uri, content).Result.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
                return Shared.Newtonsoft.Serializer.Deserialize<T>(reader);
        }

        /// <summary>
        /// Make a POST Request and get response as specified type
        /// </summary>
        /// <param name="uri">Intended Target</param>
        /// <param name="content">HttpContent Data</param>
        /// <returns>Type containing response data</returns>
        public static T PostData<T>(string uri, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
                return PostData<T>(uri, content, client);
        }
    }
}