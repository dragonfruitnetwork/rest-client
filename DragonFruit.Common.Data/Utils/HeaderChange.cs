// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Common.Data.Utils
{
    internal class HeaderChange
    {
        public HeaderChange(string key, string value, bool remove)
        {
            Key = key;
            Value = value;
            Remove = remove;
        }

        public string Key { get; set; }

        public string Value { get; set; }

        public bool Remove { get; set; }
    }
}
