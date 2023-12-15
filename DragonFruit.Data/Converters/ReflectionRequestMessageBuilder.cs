// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Serializers;

namespace DragonFruit.Data.Converters
{
    public static class ReflectionRequestMessageBuilder
    {
        public static HttpRequestMessage CreateHttpRequestMessage(ApiRequest request, SerializerResolver serializers)
        {
            var requestType = request.GetType();
            var requestProperties = requestType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var requestParams = requestProperties.Select(GetPropertyInfo).Where(x => x != null).ToLookup(x => x.Value.ParameterType, x => (PropertyName: x.Value.ParameterName, x.Value.Accessor));

            var requestUri = new UriBuilder(request.RequestPath);

            // build query
            if (requestParams[ParameterType.Query].Any())
            {
                var queryBuilder = new StringBuilder();

                foreach (var queryParameter in requestParams[ParameterType.Query])
                {
                    WriteUriProperty(queryBuilder, queryParameter.PropertyName, queryParameter.Accessor, request);
                }

                if (queryBuilder.Length > 0)
                {
                    // trim trailing &
                    queryBuilder.Length--;
                    requestUri.Query = queryBuilder.ToString();
                }
            }

            var requestMessage = new HttpRequestMessage(request.RequestMethod, requestUri.Uri);

            // add headers
            foreach (var headerParameter in requestParams[ParameterType.Header])
            {
                WriteHeaderProperty(requestMessage.Headers, headerParameter.PropertyName, headerParameter.Accessor, request);
            }

            // check if there are any form params, if not, then check for a body property
            if (!requestParams[ParameterType.Form].Any())
            {
                var bodyProperty = requestProperties.FirstOrDefault(x => x.GetCustomAttribute<RequestBodyAttribute>() != null);

                if (bodyProperty != null)
                {
                    var bodyContent = bodyProperty.GetValue(request);
                    requestMessage.Content = bodyContent switch
                    {
                        HttpContent httpContent => httpContent,
                        Stream stream => new StreamContent(stream),
                        byte[] byteArray => new ByteArrayContent(byteArray),

                        _ => serializers.Resolve(bodyProperty.PropertyType, DataDirection.Out).Serialize(bodyContent)
                    };
                }
            }
            else
            {
                switch (requestType.GetCustomAttribute<FormBodyTypeAttribute>()?.BodyType ?? FormBodyType.UriEncoded)
                {
                    case FormBodyType.UriEncoded:
                    {
                        var formBuilder = new StringBuilder();

                        foreach (var formParameter in requestParams[ParameterType.Form])
                        {
                            WriteUriProperty(formBuilder, formParameter.PropertyName, formParameter.Accessor, request);
                        }

                        if (formBuilder.Length > 0)
                        {
                            // trim trailing &
                            formBuilder.Length--;
                        }

                        requestMessage.Content = new StringContent(formBuilder.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
                        break;
                    }

                    case FormBodyType.Multipart:
                    {
                        var multipartForm = new MultipartFormDataContent();

                        foreach (var formParameter in requestParams[ParameterType.Form])
                        {
                            WriteMultipartProperty(multipartForm, formParameter.PropertyName, formParameter.Accessor, request);
                        }

                        requestMessage.Content = multipartForm;
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return requestMessage;
        }

        private static void WriteUriProperty(StringBuilder destination, string parameterName, PropertyInfo accessor, object source)
        {
            var propertyValue = accessor.GetValue(source);

            if (accessor.PropertyType.IsEnum)
            {
                var options = accessor.GetCustomAttribute<EnumOptionsAttribute>()?.Options ?? EnumOption.None;
                EnumConverter.WriteEnum(destination, (Enum)propertyValue, options, parameterName);

                return;
            }

            switch (propertyValue)
            {
                case string _:
                    goto default;

                // KeyValuePair<string, string>
                case IEnumerable<KeyValuePair<string, string>> dynamicPairs:
                    KeyValuePairConverter.WriteKeyValuePairs(destination, dynamicPairs);
                    break;

                // any enumerable
                case IEnumerable enumerable:
                {
                    var options = accessor.GetCustomAttribute<EnumerableOptionsAttribute>();
                    EnumerableConverter.WriteEnumerable(destination, enumerable, options?.Options, parameterName, options?.Separator);
                    break;
                }

                // default handling
                default:
                    destination.Append($"{parameterName}={Uri.EscapeDataString(propertyValue.ToString())}&");
                    break;
            }
        }

        private static void WriteHeaderProperty(HttpHeaders collection, string parameterName, PropertyInfo accessor, object source)
        {
            var propertyValue = accessor.GetValue(source);

            if (accessor.PropertyType.IsEnum)
            {
                var options = accessor.GetCustomAttribute<EnumOptionsAttribute>()?.Options ?? EnumOption.None;
                collection.Add(parameterName, EnumConverter.GetEnumValue((Enum)propertyValue, options));
                return;
            }

            switch (propertyValue)
            {
                case string _:
                    goto default;

                case IEnumerable<KeyValuePair<string, string>> dynamicPairs:
                {
                    foreach (var kvp in dynamicPairs)
                    {
                        collection.Add(kvp.Key, kvp.Value);
                    }

                    break;
                }

                case IEnumerable enumerable:
                {
                    var options = accessor.GetCustomAttribute<EnumerableOptionsAttribute>();

                    foreach (var kvp in EnumerableConverter.GetPairs(enumerable, options?.Options, parameterName, options?.Separator))
                    {
                        collection.Add(kvp.Key, kvp.Value);
                    }

                    break;
                }

                default:
                    collection.Add(parameterName, propertyValue.ToString());
                    break;
            }
        }

        private static void WriteMultipartProperty(MultipartFormDataContent multipartForm, string parameterName, PropertyInfo accessor, object source)
        {
            var value = accessor.GetValue(source);

            if (accessor.PropertyType.IsEnum)
            {
                var options = accessor.GetCustomAttribute<EnumOptionsAttribute>()?.Options ?? EnumOption.None;
                multipartForm.Add(new StringContent(EnumConverter.GetEnumValue((Enum)value, options)), parameterName);
            }
            else
            {
                switch (value)
                {
                    case string _:
                        goto default;

                    case Stream stream:
                        multipartForm.Add(new StreamContent(stream), parameterName);
                        break;

                    case byte[] byteArray:
                        multipartForm.Add(new ByteArrayContent(byteArray), parameterName);
                        break;

                    case IEnumerable<KeyValuePair<string, string>> dynamicPairs:
                    {
                        foreach (var kvp in dynamicPairs)
                        {
                            multipartForm.Add(new StringContent(kvp.Value), kvp.Key);
                        }

                        break;
                    }

                    case IEnumerable enumerable:
                    {
                        var options = accessor.GetCustomAttribute<EnumerableOptionsAttribute>();

                        foreach (var kvp in EnumerableConverter.GetPairs(enumerable, options?.Options, parameterName, options?.Separator))
                        {
                            multipartForm.Add(new StringContent(kvp.Value), kvp.Key);
                        }

                        break;
                    }

                    default:
                        multipartForm.Add(new StringContent(value.ToString()), parameterName);
                        break;
                }
            }
        }

        private static (ParameterType ParameterType, string ParameterName, PropertyInfo Accessor)? GetPropertyInfo(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<RequestParameterAttribute>();

            if (attribute != null)
            {
                return (attribute.ParameterType, attribute.Name ?? property.Name, property);
            }

            return null;
        }
    }
}
