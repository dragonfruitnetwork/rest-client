// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using DragonFruit.Common.Data.Parameters;
using DragonFruit.Common.Data.Serializers;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ApiRequest
    {
        public abstract string Path { get; }

        protected virtual Methods Method => Methods.Get;

        protected virtual DataTypes DataType { get; }

        public virtual bool RequireAuth => false;

        public virtual string AcceptedContent => string.Empty;

        public string FullUrl => Path + QueryString;

        public virtual HttpContent BodyContent { get; }

        /// <summary>
        /// <see cref="CultureInfo"/> used for ToString() conversions when collecting attributed members/>
        /// </summary>
        protected virtual CultureInfo Culture => CultureInfo.InvariantCulture;

        internal string QueryString
        {
            get
            {
                var queries = GetParameter<QueryParameter>();
                return !queries.Any()
                    ? string.Empty
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

        internal object GetSingleParameterObject<T>() where T : Attribute
        {
            var property = GetType().GetProperties()
                                    .Single(x => Attribute.GetCustomAttribute(x, typeof(T)) is T);

            return property.GetValue(this, null);
        }

        /// <summary>
        /// Creates the default <see cref="HttpResponseMessage"/>, which can then be overriden by <see cref="SetupRequest"/>
        /// </summary>
        internal HttpRequestMessage GetRequest(ISerializer serializer)
        {
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
                    request.Method = new HttpMethod("PATCH"); //in .NET standard 2 patch isn't implemented...
                    request.Content = GetContent(serializer);
                    break;

                case Methods.Delete:
                    request.Method = HttpMethod.Delete;
                    request.Content = GetContent(serializer);
                    break;

                case Methods.Head:
                    request.Method = HttpMethod.Head;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return request;
        }

        private HttpContent GetContent(ISerializer serializer)
        {
            switch (DataType)
            {
                case DataTypes.Encoded:
                    return new FormUrlEncodedContent(GetParameter<FormParameter>());

                case DataTypes.Serialized:
                    return serializer.Serialize(this);

                case DataTypes.SerializedProperty:
                    var body = serializer.Serialize(GetSingleParameterObject<RequestBody>());
                    return body;

                case DataTypes.Custom:
                    return BodyContent;

                default:
                    //todo custom exception - there should have been a datatype specified
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal ApiRequest Clone() => (ApiRequest)MemberwiseClone();
    }
}
