// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using DragonFruit.Common.Data.Exceptions;
using DragonFruit.Common.Data.Parameters;
using DragonFruit.Common.Data.Serializers;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ApiRequest
    {
        /// <summary>
        /// The Path to the web resource
        /// </summary>
        public abstract string Path { get; }

        /// <summary>
        /// The method to use/request verb
        /// </summary>
        protected virtual Methods Method => Methods.Get;

        /// <summary>
        /// The <see cref="BodyType"/> to use (if there is a body to be sent)
        /// </summary>
        protected virtual BodyType BodyType { get; }

        /// <summary>
        /// Whether an auth header is required.
        /// </summary>
        /// <exception cref="ClientValidationException">This was set to true but no auth header was specified.
        /// Automatically suppressed if the <see cref="Headers"/> property has been initialised.
        /// </exception>
        protected internal virtual bool RequireAuth => false;

        /// <summary>
        /// Custom Headers to send with this request. Overrides any custom header set in the <see cref="HttpClient"/> with the same name.
        /// </summary>
        /// <remarks>
        /// Headers to be set in all requests should be set at <see cref="ApiClient"/>-level, using the <see cref="ApiClient.Headers"/> Dictionary.
        /// </remarks>
        public Lazy<IDictionary<string, string>> Headers { get; set; } = new Lazy<IDictionary<string, string>>(() => new Dictionary<string, string>());

        /// <summary>
        /// The fully compiled url
        /// </summary>
        public string FullUrl => Path + QueryString;

        /// <summary>
        /// Overridable property for configuring a custom body for this request
        ///
        /// <para>
        /// Only used when the <see cref="BodyType"/> is equal to <see cref="BodyType.Custom"/>
        /// </para>
        /// </summary>
        protected virtual HttpContent BodyContent { get; }

        /// <summary>
        /// <see cref="CultureInfo"/> used for ToString() conversions when collecting attributed members
        /// </summary>
        protected virtual CultureInfo Culture => CultureInfo.InvariantCulture;

        internal string QueryString
        {
            get
            {
                var queries = GetParameter<QueryParameter>();
                return !queries.Any()
                    ? null
                    : $"?{string.Join("&", queries.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
            }
        }

        internal IEnumerable<KeyValuePair<string, string>> GetParameter<T>() where T : IProperty
        {
            var type = typeof(T);

            foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (!(Attribute.GetCustomAttribute(property, type) is T parameter))
                {
                    continue;
                }

                var value = property.GetValue(this, null);
                string convertedValue = value switch
                {
                    bool boolVar => boolVar.ToString().ToLower(Culture),
                    null => null,

                    _ => value.ToString()
                };

                if (convertedValue != null)
                {
                    yield return new KeyValuePair<string, string>(parameter.Name, convertedValue);
                }
            }
        }

        internal object GetSingleParameterObject<T>() where T : Attribute =>
            GetType().GetProperties()
                     .Single(x => Attribute.GetCustomAttribute(x, typeof(T)) is T)
                     .GetValue(this, null);

        public HttpRequestMessage Build(ApiClient client) => Build(client.Serializer);

        /// <summary>
        /// Creates a <see cref="HttpResponseMessage"/> for this <see cref="ApiRequest"/>, which can then be modified manually or overriden by <see cref="ApiClient.SetupRequest"/>
        /// </summary>
        /// <remarks>
        /// This validates the <see cref="Path"/> and <see cref="RequireAuth"/> properties, throwing a <see cref="ClientValidationException"/> if it's unsatisfied with the constraints
        /// </remarks>
        public HttpRequestMessage Build(ISerializer serializer)
        {
            if (!Path.StartsWith("http"))
            {
                throw new HttpRequestException("The request path is invalid (it must start with http or https)");
            }

            var request = new HttpRequestMessage { RequestUri = new Uri(FullUrl) };

            //generic setup
            switch (Method)
            {
                case Methods.Get:
                    request.Method = HttpMethod.Get;
                    break;

                case Methods.Post:
                    request.Method = HttpMethod.Post;
                    request.Content = GetContent(serializer);
                    break;

                case Methods.Put:
                    request.Method = HttpMethod.Put;
                    request.Content = GetContent(serializer);
                    break;

                case Methods.Patch:
                    request.Method = new HttpMethod("PATCH"); //in .NET Standard 2.0 patch isn't implemented...
                    request.Content = GetContent(serializer);
                    break;

                case Methods.Delete:
                    request.Method = HttpMethod.Delete;
                    request.Content = GetContent(serializer);
                    break;

                case Methods.Head:
                    request.Method = HttpMethod.Head;
                    break;

                case Methods.Trace:
                    request.Method = HttpMethod.Trace;
                    break;

                default:
                    throw new NotImplementedException();
            }

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(serializer.ContentType));

            if (Headers.IsValueCreated)
            {
                foreach (var header in Headers.Value)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return request;
        }

        private HttpContent GetContent(ISerializer serializer)
        {
            switch (BodyType)
            {
                case BodyType.Encoded:
                    return new FormUrlEncodedContent(GetParameter<FormParameter>());

                case BodyType.Serialized:
                    return serializer.Serialize(this);

                case BodyType.SerializedProperty:
                    var body = serializer.Serialize(GetSingleParameterObject<RequestBody>());
                    return body;

                case BodyType.Custom:
                    return BodyContent;

                default:
                    //todo custom exception - there should have been a datatype specified
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
