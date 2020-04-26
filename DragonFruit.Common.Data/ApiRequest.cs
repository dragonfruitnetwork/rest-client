// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DragonFruit.Common.Data.Parameters;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ApiRequest
    {
        public abstract string Path { get; }

        public virtual Methods Method => Methods.Get;

        public virtual DataTypes DataType { get; }

        public virtual bool RequireAuth => false;

        public virtual string AcceptedContent => string.Empty;

        public string FullUrl => Path + QueryString;

        public virtual HttpContent BodyContent { get; }

        public string QueryString
        {
            get
            {
                var queries = GetParameter<QueryParameter>();
                return !queries.Any()
                    ? string.Empty
                    : $"?{string.Join("&", queries.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetParameter<T>() where T : IProperty
        {
            var type = typeof(T);

            foreach (var property in GetType().GetProperties())
            {
                if (!(Attribute.GetCustomAttribute(property, type) is T parameter))
                    continue;

                var value = property.GetValue(this, null);
                if (value != null)
                    yield return new KeyValuePair<string, string>(parameter.Name, value.ToString());
            }
        }

        public object GetSingleParameterObject<T>() where T : Attribute
        {
            var property = GetType().GetProperties()
                                    .Single(x => Attribute.GetCustomAttribute(x, typeof(T)) is T);

            return property.GetValue(this, null);
        }
    }
}
