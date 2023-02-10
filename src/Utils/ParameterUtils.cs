// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DragonFruit.Data.Parameters;

namespace DragonFruit.Data.Utils
{
    public static class ParameterUtils
    {
        private const string DefaultConcatenationCharacter = ",";

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s from properties with a specified <see cref="IProperty"/>-inheriting attribute.
        /// </summary>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameter<T>(object host, CultureInfo culture) where T : IProperty
        {
            foreach (var property in host.GetType().GetTargetProperties())
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
                    Func<IEnumerable, IEnumerable<KeyValuePair<string, string>>> entityConverter = attribute.CollectionHandling switch
                    {
                        CollectionConversionMode.Ordered => values => ApplyOrderedConversion(values, keyName, culture),
                        CollectionConversionMode.Recursive => values => values.Cast<object>().Select(x => x.ToKeyValuePair(keyName, culture)),
                        CollectionConversionMode.Unordered => values => values.Cast<object>().Select(x => x.ToKeyValuePair($"{keyName}[]", culture)),
                        CollectionConversionMode.Concatenated => values => ApplyConcatenation(values, keyName, culture, attribute.CollectionSeparator ?? DefaultConcatenationCharacter),

                        _ => throw new ArgumentOutOfRangeException()
                    };

                    // we purposely keep nulls in here, as it might affect the ordering.
                    foreach (var entry in entityConverter.Invoke((IEnumerable)propertyValue))
                    {
                        yield return entry;
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    switch (attribute.EnumHandling)
                    {
                        case EnumHandlingMode.Numeric:
                            yield return ((int)propertyValue).ToKeyValuePair(keyName, culture);
                            break;

                        case EnumHandlingMode.StringLower:
                            yield return propertyValue.ToString().ToLower(culture).Replace(" ", string.Empty).ToKeyValuePair(keyName, culture);
                            break;

                        case EnumHandlingMode.StringUpper:
                            yield return propertyValue.ToString().ToUpper(culture).Replace(" ", string.Empty).ToKeyValuePair(keyName, culture);
                            break;

                        default:
                            yield return propertyValue.ToKeyValuePair(keyName, culture);
                            break;
                    }
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
                                         .GetTargetProperties()
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

        private static IEnumerable<KeyValuePair<string, string>> ApplyOrderedConversion(IEnumerable values, string keyName, CultureInfo culture)
        {
            var counter = 0;
            var enumerator = values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current.ToKeyValuePair($"{keyName}[{counter++}]", culture);
            }

            // dispose if possible
            (enumerator as IDisposable)?.Dispose();
        }

        private static IEnumerable<PropertyInfo> GetTargetProperties(this Type target)
        {
#if NET6_0 && ANDROID
            // android has an issue where nonpublic properties aren't returned from base classes (see https://github.com/dotnet/runtime/pull/77169)
            var props = target.GetRuntimeProperties();
            var baseType = target.BaseType;

            while (baseType != null && baseType != typeof(ApiRequest))
            {
                props = props.Concat(baseType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
                baseType = baseType.BaseType;
            }

            return props;
#else
            return target.GetRuntimeProperties();
#endif
        }

        private static IEnumerable<KeyValuePair<string, string>> ApplyConcatenation(IEnumerable values, string keyName, CultureInfo culture, string concatCharacter)
        {
            yield return new KeyValuePair<string, string>(keyName, string.Join(concatCharacter, values.Cast<object>().Select(x => x.AsString(culture))));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static KeyValuePair<string, string> ToKeyValuePair(this object value, string key, CultureInfo culture)
        {
            return new KeyValuePair<string, string>(key, value.AsString(culture));
        }
    }
}
