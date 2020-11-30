// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;

namespace DragonFruit.Common.Data.Basic
{
    public interface IBasicApiRequest
    {
        /// <summary>
        /// Collection of <see cref="KeyValuePair{TKey,TValue}"/>s to use as a query string
        /// </summary>
        public Lazy<List<KeyValuePair<string, string>>> Queries { get; }
    }
}
