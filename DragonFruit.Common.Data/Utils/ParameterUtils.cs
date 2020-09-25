// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
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
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s from properties with a specified <see cref="IProperty"/>-inheriting attribute.
        /// </summary>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameter<T>(object host, CultureInfo culture) where T : IProperty
        {
            var type = typeof(T);

            foreach (var property in host.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (!(Attribute.GetCustomAttribute(property, type) is T parameter))
                {
                    continue;
                }

                var value = property.GetValue(host, null);
                string convertedValue = value switch
                {
                    bool boolVar => boolVar.ToString().ToLower(culture),
                    null => null,

                    _ => value.ToString()
                };

                if (convertedValue != null)
                {
                    yield return new KeyValuePair<string, string>(parameter.Name, convertedValue);
                }
            }
        }

        /// <summary>
        /// Gets the single attibute of its kind from a class.
        /// </summary>
        internal static object GetSingleParameterObject<T>(object host) where T : Attribute
        {
            return host.GetType()
                       .GetProperties()
                       .Single(x => Attribute.GetCustomAttribute(x, typeof(T)) is T)
                       .GetValue(host, null);
        }
    }
}
