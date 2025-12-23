// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Requests
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class EnumerableOptionsAttribute(EnumerableOption options) : Attribute
    {
        public EnumerableOptionsAttribute(EnumerableOption options, string separator)
            : this(options)
        {
            Separator = separator;
        }

        public EnumerableOption Options { get; } = options;
        public string? Separator { get; }
    }
}
