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
        /// Default <see cref="BindingFlags"/> to search for matching properties
        /// </summary>
        internal const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s from properties with a specified <see cref="IProperty"/>-inheriting attribute.
        /// </summary>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameter<T>(object host, CultureInfo culture) where T : IProperty
        {
            var type = typeof(T);

            foreach (var property in host.GetType().GetProperties(DefaultFlags))
            {
                if (!(Attribute.GetCustomAttribute(property, type) is T parameter))
                {
                    continue;
                }

                var convertedValue = property.GetValue(host).AsString(culture);
                if (convertedValue != null)
                    yield return new KeyValuePair<string, string>(parameter.Name, convertedValue);
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
