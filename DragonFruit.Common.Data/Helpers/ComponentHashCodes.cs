// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

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