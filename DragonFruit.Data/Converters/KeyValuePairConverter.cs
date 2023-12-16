// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Text;

namespace DragonFruit.Data.Converters
{
    public class KeyValuePairConverter
    {
        public static void WriteKeyValuePairs(StringBuilder destination, IEnumerable<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
            {
                destination.AppendFormat("{0}={1}&", pair.Key, Uri.EscapeDataString(pair.Value));
            }
        }
    }
}
