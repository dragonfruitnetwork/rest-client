using System;
using System.Collections.Generic;
using System.Text;

namespace DragonFruit.Common.Data.Helpers
{
    public static class ComponentHashCodes
    {
        public static string ItemHashCode(this object data) => data == null ? "!" : data.GetHashCode().ToString();
    }
}
