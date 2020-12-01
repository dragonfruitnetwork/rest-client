// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using DragonFruit.Common.Data.Parameters;
using DragonFruit.Common.Data.Serializers;
using DragonFruit.Common.Data.Utils;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ApiRequest
    {
        private List<KeyValuePair<string, string>> _headers;

        /// <summary>
        /// The path (including host, protocol and non-standard port) to the web resource
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
        public List<KeyValuePair<string, string>> Headers => _headers ??= new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Internal check for whether the custom header collection (<see cref="Headers"/>) has been initialised
        /// </summary>
        internal bool CustomHeaderCollectionCreated => _headers != null;

        /// <summary>
        /// The fully compiled url
        /// </summary>
        public string FullUrl => UrlCompiler;

        /// <summary>
        /// Getter for fully compiled url (internally visible)
        /// </summary>
        internal virtual string UrlCompiler => Path + QueryString;

        /// <summary>
        /// Overridable property for configuring a custom body for this request
        /// <para>
        /// Only used when the <see cref="BodyType"/> is equal to <see cref="BodyType.Custom"/>
        /// </para>
        /// </summary>
        protected virtual HttpContent BodyContent { get; }

        /// <summary>
        /// Overridable culture for serialising requests.
        /// Defaults to <see cref="CultureUtils.DefaultCulture"/>
        /// </summary>
        protected virtual CultureInfo RequestCulture => CultureUtils.DefaultCulture;

        /// <summary>
        /// Query string generated from all filled <see cref="QueryParameter"/>-attributed properties
        /// </summary>
        internal string QueryString => QueryUtils.QueryStringFrom(ParameterUtils.GetParameter<QueryParameter>(this, RequestCulture));

        /// <summary>
        /// Create a <see cref="HttpResponseMessage"/> for this <see cref="ApiRequest"/>, which can then be modified manually or overriden by <see cref="ApiClient.SetupRequest"/>
        /// </summary>
        public HttpRequestMessage Build(ApiClient client) => Build(client.Serializer);

        /// <summary>
        /// Create a <see cref="HttpResponseMessage"/> for this <see cref="ApiRequest"/>, which can then be modified manually or overriden by <see cref="ApiClient.SetupRequest"/>
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
#if NET5_0
                    request.Method = HttpMethod.Patch;
#else
                    // .NET Standard 2.0 doesn't have a PATCH method...
                    request.Method = new HttpMethod("PATCH");
#endif
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

            if (CustomHeaderCollectionCreated)
            {
                foreach (var header in Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (!request.Headers.Contains("Accept"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(serializer.ContentType));
            }

            return request;
        }

        private HttpContent GetContent(ISerializer serializer)
        {
            switch (BodyType)
            {
                case BodyType.Encoded:
                    return new FormUrlEncodedContent(ParameterUtils.GetParameter<FormParameter>(this, RequestCulture));

                case BodyType.Serialized:
                    return serializer.Serialize(this);

                case BodyType.SerializedProperty:
                    var body = serializer.Serialize(ParameterUtils.GetSingleParameterObject<RequestBody>(this));
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
