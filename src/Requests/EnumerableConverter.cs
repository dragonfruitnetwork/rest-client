﻿// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace DragonFruit.Data.Requests
{
    public static class EnumerableConverter
    {
        /// <summary>
        /// Writes the provided <see cref="IEnumerable"/> to the <see cref="StringBuilder"/> using the specified <see cref="EnumerableOption"/>
        /// </summary>
        /// <param name="destination">The destination <see cref="StringBuilder"/></param>
        /// <param name="source">The source collection</param>
        /// <param name="mode">The <see cref="EnumerableOption"/> to use. If none provided, defaults to <see cref="EnumerableOption.Concatenated"/></param>
        /// <param name="propertyName">The name of the property to use when writing values to <see cref="destination"/></param>
        /// <param name="separator">The separator to use, if required.</param>
        public static void WriteEnumerable(StringBuilder destination, IEnumerable source, EnumerableOption mode, string propertyName, string separator)
        {
            switch (mode)
            {
                case EnumerableOption.Recursive:
                {
                    foreach (var item in source)
                    {
                        destination.Append($"{propertyName}={Uri.EscapeDataString(item.ToString())}&");
                    }

                    break;
                }

                case EnumerableOption.Indexed:
                {
                    var counter = 0;

                    foreach (var item in source)
                    {
                        destination.AppendFormat("{0}[{1}]={2}&", propertyName, counter++, Uri.EscapeDataString(item.ToString()));
                    }

                    break;
                }

                case EnumerableOption.Unordered:
                {
                    foreach (var item in source)
                    {
                        destination.AppendFormat("{0}[]={1}&", propertyName, Uri.EscapeDataString(item.ToString()));
                    }

                    break;
                }

                default:
                {
                    destination.AppendFormat("{0}={1}&", propertyName, string.Join(separator, source.Cast<object>().Select(x => Uri.EscapeDataString(x.ToString()))));
                    break;
                }
            }
        }
    }
}
