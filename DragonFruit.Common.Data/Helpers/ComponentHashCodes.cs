using System;
using System.Collections.Generic;
using System.Text;

namespace DragonFruit.Common.Data.Helpers
{
    internal static class ComponentHashCodes
    {
        internal static string ItemHashCode(this object data) => data == null ? "!" : data.GetHashCode().ToString();
    }
}
