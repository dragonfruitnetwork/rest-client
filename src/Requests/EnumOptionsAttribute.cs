// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Requests
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class EnumOptionsAttribute : Attribute
    {
        public EnumOptionsAttribute(EnumOption options)
        {
            Options = options;
        }

        public EnumOption Options { get; set; }
    }
}
