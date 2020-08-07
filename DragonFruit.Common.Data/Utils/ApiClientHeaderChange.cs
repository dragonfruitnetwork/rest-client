﻿// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Common.Data.Utils
{
    internal class ApiClientHeaderChange
    {
        public ApiClientHeaderChange(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
