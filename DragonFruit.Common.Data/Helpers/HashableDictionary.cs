// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.Linq;

namespace DragonFruit.Common.Data.Helpers
{
    /// <summary>
    /// A superset of <see cref="T:System.Collections.Generic.Dictionary`2" /> with a hash code function that calculates the hash based on the keys and values inside.
    /// </summary>
    public class HashableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public override int GetHashCode()
        {
            int hash = Count;

            unchecked //doesn't matter if we get an overflow here
            {
                hash += Keys.Sum(item => item.GetHashCode() * 5);
                hash += Values.Sum(item => item.GetHashCode() * 6);
            }

            return hash;
        }
    }
}
