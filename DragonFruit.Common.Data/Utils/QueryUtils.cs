// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.Common.Data.Utils
{
    internal static class QueryUtils
    {
        /// <summary>
        /// Produces a query string from an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s
        /// </summary>
        public static string QueryStringFrom(IEnumerable<KeyValuePair<string, string>> queries) => !queries.Any()
            ? string.Empty
            : $"?{string.Join("&", queries.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

        public static IEnumerable<KeyValuePair<string, string>> QueryDataFrom(object host, CultureInfo info)
        {
            foreach (var property in host.GetType().GetProperties(ParameterUtils.DefaultFlags))
            {
                if (!(Attribute.GetCustomAttribute(property, typeof(QueryParameter)) is QueryParameter parameter))
                {
                    continue;
                }

                // check if the type we've got is an IEnumerable of anything (in this case object)
                if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    var values = (IEnumerable)property.GetValue(host);

                    switch (parameter.CollectionHandling)
                    {
                        case CollectionConversionMode.Recursive:
                        {
                            foreach (var item in values)
                            {
                                // return multiple of the same key
                                yield return new KeyValuePair<string, string>(parameter.Name, item.AsString(info));
                            }

                            break;
                        }

                        case CollectionConversionMode.Unordered:
                        {
                            foreach (var item in values)
                            {
                                // return multiple suffixed keys
                                yield return new KeyValuePair<string, string>($"{parameter.Name}[]", item.AsString(info));
                            }

                            break;
                        }

                        // if it's order-explicit we need to use an enumerator as there's no length count
                        case CollectionConversionMode.Ordered:
                        {
                            var counter = 0;
                            var enumerator = values.GetEnumerator();

                            while (enumerator.MoveNext())
                            {
                                // return suffixed version with counter
                                yield return new KeyValuePair<string, string>($"{parameter.Name}[{counter}]", enumerator.Current.AsString(info));
                                counter++;
                            }

                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // anything else can just be taken as a single object for now
                    var convertedValue = property.GetValue(host).AsString(info);
                    if (convertedValue != null)
                        yield return new KeyValuePair<string, string>(parameter.Name, convertedValue);
                }
            }
        }
    }
}
