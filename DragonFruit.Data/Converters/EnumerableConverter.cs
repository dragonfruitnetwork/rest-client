// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Converters
{
    public static class EnumerableConverter
    {
        private const EnumerableOption DefaultOption = EnumerableOption.Concatenated;
        private const string DefaultSeparator = ",";

        /// <summary>
        /// Writes the provided <see cref="IEnumerable"/> to the <see cref="StringBuilder"/> using the specified <see cref="EnumerableOption"/>
        /// </summary>
        /// <param name="destination">The destination <see cref="StringBuilder"/></param>
        /// <param name="source">The source collection</param>
        /// <param name="mode">The <see cref="EnumerableOption"/> to use. If none provided, defaults to <see cref="EnumerableOption.Concatenated"/></param>
        /// <param name="parameterName">The name of the parameter to use when writing values to <see cref="destination"/></param>
        /// <param name="separator">The separator to use, if required.</param>
        public static void WriteEnumerable(StringBuilder destination, IEnumerable source, EnumerableOption? mode, string parameterName, string separator)
        {
            switch (mode ?? DefaultOption)
            {
                case EnumerableOption.Recursive:
                {
                    foreach (var item in source)
                    {
                        destination.Append($"{parameterName}={Uri.EscapeDataString(item.ToString())}&");
                    }

                    break;
                }

                case EnumerableOption.Indexed:
                {
                    var counter = 0;

                    foreach (var item in source)
                    {
                        destination.AppendFormat("{0}[{1}]={2}&", parameterName, counter++, Uri.EscapeDataString(item.ToString()));
                    }

                    break;
                }

                case EnumerableOption.Unordered:
                {
                    foreach (var item in source)
                    {
                        destination.AppendFormat("{0}[]={1}&", parameterName, Uri.EscapeDataString(item.ToString()));
                    }

                    break;
                }

                default:
                {
                    destination.AppendFormat("{0}={1}&", parameterName, string.Join(separator ?? DefaultSeparator, source.Cast<object>().Select(x => Uri.EscapeDataString(x.ToString()))));
                    break;
                }
            }
        }

        /// <summary>
        /// Produces a collection of <see cref="KeyValuePair{TKey,TValue}"/> from the provided <see cref="IEnumerable"/> using the specified <see cref="EnumerableOption"/>
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable"/> to derive pairs from</param>
        /// <param name="mode">The <see cref="EnumerableOption"/> to use</param>
        /// <param name="parameterName">The name of the parameter to use</param>
        /// <param name="separator">The separator to use, if <see cref="mode"/> is set to Concatenated</param>
        public static IEnumerable<KeyValuePair<string, string>> GetPairs(IEnumerable source, EnumerableOption? mode, string parameterName, string separator)
        {
            switch (mode ?? DefaultOption)
            {
                case EnumerableOption.Recursive:
                {
                    foreach (var item in source)
                    {
                        yield return new KeyValuePair<string, string>(parameterName, Uri.EscapeDataString(item.ToString()));
                    }

                    break;
                }

                case EnumerableOption.Indexed:
                {
                    var counter = 0;

                    foreach (var item in source)
                    {
                        yield return new KeyValuePair<string, string>($"{parameterName}[{counter++}]", Uri.EscapeDataString(item.ToString()));
                    }

                    break;
                }

                case EnumerableOption.Unordered:
                {
                    foreach (var item in source)
                    {
                        yield return new KeyValuePair<string, string>($"{parameterName}[]", Uri.EscapeDataString(item.ToString()));
                    }

                    break;
                }

                default:
                {
                    yield return new KeyValuePair<string, string>(parameterName, string.Join(separator ?? DefaultSeparator, source.Cast<object>().Select(x => Uri.EscapeDataString(x.ToString()))));

                    break;
                }
            }
        }
    }
}
