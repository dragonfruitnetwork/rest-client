using System;
using System.Collections.Generic;

namespace DragonFruit.Common.Data.Utils
{
    internal static class DictionaryHelpers
    {
        /// <summary>
        /// Try to get a value from a dictionary, creating and inserting it if it doesn't already exist
        /// </summary>
        internal static TValue GetOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> set)
        {
            if (dict.TryGetValue(key, out var val))
            {
                return val;
            }

            var output = set.Invoke();

            dict.Add(key, output);
            return output;
        }
    }
}
