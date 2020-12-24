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
        private const string DefaultConcatenationCharacter = ",";

        /// <summary>
        /// Default <see cref="BindingFlags"/> to search for matching properties
        /// </summary>
        private const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s from properties with a specified <see cref="IProperty"/>-inheriting attribute.
        /// </summary>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameter<T>(object host, CultureInfo culture) where T : IProperty
        {
            foreach (var property in host.GetType().GetProperties(DefaultFlags))
            {
                if (!property.CanRead || !(Attribute.GetCustomAttribute(property, typeof(T)) is T attribute))
                {
                    continue;
                }

                var keyName = attribute.Name ?? property.Name;
                var propertyValue = property.GetValue(host);

                if (propertyValue == null)
                {
                    // ignore null values
                    continue;
                }

                // check if the type we've got is an IEnumerable of anything AND we have a valid collection handler mode
                if (attribute.CollectionHandling.HasValue && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    Func<IEnumerable<object>, string, CultureInfo, IEnumerable<KeyValuePair<string, string>>> entityConverter = attribute.CollectionHandling switch
                    {
                        CollectionConversionMode.Recursive => ApplyRecursiveConversion,
                        CollectionConversionMode.Unordered => ApplyUnorderedConversion,
                        CollectionConversionMode.Ordered => ApplyOrderedConversion,
                        CollectionConversionMode.Concatenated => (a, b, c) => ApplyConcatenation(a, b, c, attribute.CollectionSeparator ?? DefaultConcatenationCharacter),

                        _ => throw new ArgumentOutOfRangeException()
                    };

                    foreach (var entry in entityConverter.Invoke((IEnumerable<object>)propertyValue, keyName, culture))
                    {
                        // we purposely keep nulls in here, as it might affect the ordering.
                        yield return entry;
                    }
                }
                else if (property.PropertyType.IsEnum && attribute.EnumHandling.HasValue)
                {
                    yield return attribute.EnumHandling.Value switch
                    {
                        EnumHandlingMode.Numeric => ((int)propertyValue).ToKeyValuePair(keyName, culture),
                        EnumHandlingMode.StringLower => propertyValue.ToString().ToLower(culture).ToKeyValuePair(keyName, culture),

                        // default includes string handling
                        _ => propertyValue.ToKeyValuePair(keyName, culture)
                    };
                }
                else
                {
                    yield return propertyValue.ToKeyValuePair(keyName, culture);
                }
            }
        }

        /// <summary>
        /// Gets the single attribute of its kind from a class.
        /// </summary>
        internal static object GetSingleParameterObject<T>(object host) where T : Attribute
        {
            var targetType = typeof(T);
            var attributedProperty = host.GetType()
                                         .GetProperties(DefaultFlags)
                                         .SingleOrDefault(x => Attribute.GetCustomAttribute(x, targetType) is T);

            if (attributedProperty == default)
            {
                throw new KeyNotFoundException($"No valid {targetType.Name} was attributed. There must be a single attributed property");
            }

            if (!attributedProperty.CanRead)
            {
                throw new MemberAccessException($"Unable to read contents of property {attributedProperty.Name}");
            }

            return attributedProperty.GetValue(host);
        }

        #region IEnumerable Converters

        private static IEnumerable<KeyValuePair<string, string>> ApplyRecursiveConversion(IEnumerable<object> values, string keyName, CultureInfo culture)
        {
            return values.Select(x => x.ToKeyValuePair(keyName, culture));
        }

        private static IEnumerable<KeyValuePair<string, string>> ApplyUnorderedConversion(IEnumerable<object> values, string keyName, CultureInfo culture)
        {
            return values.Select(x => x.ToKeyValuePair($"{keyName}[]", culture));
        }

        private static IEnumerable<KeyValuePair<string, string>> ApplyOrderedConversion(IEnumerable<object> values, string keyName, CultureInfo culture)
        {
            var counter = 0;
            var enumerator = values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current.ToKeyValuePair($"{keyName}[{counter}]", culture);

                counter++;
            }

            enumerator.Dispose();
        }

        private static IEnumerable<KeyValuePair<string, string>> ApplyConcatenation(IEnumerable<object> values, string keyName, CultureInfo culture, string concatCharacter)
        {
            yield return new KeyValuePair<string, string>(keyName, string.Join(concatCharacter, values.Select(x => x.AsString(culture))));
        }

        private static KeyValuePair<string, string> ToKeyValuePair(this object value, string key, CultureInfo culture)
        {
            return new KeyValuePair<string, string>(key, value.AsString(culture));
        }

        #endregion
    }
}
