// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Net.Http;
using DragonFruit.Common.API.Attributes;
using DragonFruit.Common.API.Enums;
using DragonFruit.Common.API.Serializers;

namespace DragonFruit.Common.API
{
    /// <summary>
    /// <see cref="HttpClient"/>-related data
    /// </summary>
    public class ApiClient
    {
        public string UserAgent { get; set; }

        public List<KeyValuePair<string, string>> CustomHeaders { get; set; } =
            new List<KeyValuePair<string, string>>();

        public string Authorization { get; set; } = null;

        public HttpClientHandler Handler { get; set; } = null;

        public ISerializer Serializer { get; set; } = new Json();

        public T Perform<T>(ApiRequest requestData) where T : class
        {
            //checks
            if (!(Attribute.GetCustomAttribute(requestData.GetType(), typeof(ApiPath)) is ApiPath pathInfo))
                throw new Exception("ApiPath information is missing");

            using var client = Handler != null ? new HttpClient(Handler, false) : new HttpClient();
            var url = pathInfo.Path + requestData.Query;
            var hasAuthData = !string.IsNullOrEmpty(Authorization);

            if (pathInfo.RequireAuth && !hasAuthData)
                throw new Exception("Authorization string expected, but not found");

            if (hasAuthData)
                client.DefaultRequestHeaders.Add("Authorization", Authorization);

            if (!string.IsNullOrEmpty(UserAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

            if (!string.IsNullOrEmpty(pathInfo.AcceptedContent))
                client.DefaultRequestHeaders.Accept.ParseAdd(pathInfo.AcceptedContent);

            foreach (var header in CustomHeaders)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);

            //url replacements - must contain "{0}" otherwise its irrelevant
            if (pathInfo.Path.Contains("{0}"))
            {
                foreach (var property in requestData.GetType().GetProperties())
                {
                    if (!(Attribute.GetCustomAttribute(property, typeof(UrlParameter)) is UrlParameter parameter))
                        continue;

                    var value = property.GetValue(requestData, null).ToString();
                    if (value != null)
                        url = url.Replace($"{{{parameter.Position}}}", value);
                }
            }
            
            //method specific modes and returns
            return Serializer.Deserialize<T>(pathInfo.Method switch
            {
                Methods.Get => client.GetStreamAsync(url),

                Methods.PostForm => client.PostAsync(url, requestData.FormContent).Result.Content.ReadAsStreamAsync(),
                Methods.PostString => client.PostAsync(url, Serializer.Serialize(requestData)).Result.Content.ReadAsStreamAsync(),

                Methods.PutForm => client.PutAsync(url, requestData.FormContent).Result.Content.ReadAsStreamAsync(),
                Methods.PutString => client.PutAsync(url, Serializer.Serialize(requestData)).Result.Content.ReadAsStreamAsync(),

                _ => throw new NotImplementedException()
            });
        }
    }
}