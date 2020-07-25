// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Common.Data.Helpers
{
    public static class HashExtensions
    {
        public static string ItemHashCode(this object data) => data == null ? "!" : data.GetHashCode().ToString();
    }
}
