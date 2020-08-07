// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonFruit.Common.Data.Utils
{
    /// <summary>
    /// A superset of <see cref="T:System.Collections.Generic.Dictionary`2" /> with a hash code function that calculates the hash based on the keys and values inside.
    /// </summary>
    public class HashableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IEquatable<HashableDictionary<TKey, TValue>>
    {
        public override int GetHashCode()
        {
            int hash = Count;
            hash += Keys.Sum(item => item.GetHashCode() * 5);
            hash += Values.Sum(item => item.GetHashCode() * 6);

            return hash;
        }

        #region IEquatable

        public bool Equals(HashableDictionary<TKey, TValue> other)
        {
            return other != null && GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((HashableDictionary<TKey, TValue>)obj);
        }

        #endregion
    }
}
