// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.Common.Data.Utils
{
    public static class ParameterUtils
    {
        /// <summary>
        /// Default <see cref="BindingFlags"/> to search for matching properties
        /// </summary>
        internal const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s from properties with a specified <see cref="IProperty"/>-inheriting attribute.
        /// </summary>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameter<T>(object host, CultureInfo culture) where T : IProperty
        {
            foreach (var property in host.GetType().GetProperties(ParameterUtils.DefaultFlags))
            {
                if (!(Attribute.GetCustomAttribute(property, typeof(T)) is T parameter))
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
                                yield return new KeyValuePair<string, string>(parameter.Name, item.AsString(culture));
                            }

                            break;
                        }

                        case CollectionConversionMode.Unordered:
                        {
                            foreach (var item in values)
                            {
                                // return multiple suffixed keys
                                yield return new KeyValuePair<string, string>($"{parameter.Name}[]", item.AsString(culture));
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
                                yield return new KeyValuePair<string, string>($"{parameter.Name}[{counter}]", enumerator.Current.AsString(culture));
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
                    var convertedValue = property.GetValue(host).AsString(culture);
                    yield return new KeyValuePair<string, string>(parameter.Name, convertedValue);
                }
            }
        }

        /// <summary>
        /// Gets the single attribute of its kind from a class.
        /// </summary>
        internal static object GetSingleParameterObject<T>(object host) where T : Attribute
        {
            return host.GetType()
                       .GetProperties(DefaultFlags)
                       .Single(x => Attribute.GetCustomAttribute(x, typeof(T)) is T)
                       .GetValue(host);
        }
    }
}
