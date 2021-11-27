﻿// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Common.Data.Headers
{
    internal class HeaderChange
    {
        public HeaderChange(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }
}
