// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Tests.BodySerialization
{
    internal class Employee
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("department")] public string Department { get; set; }

        [JsonProperty("manager")] public string Manager { get; set; }

        [JsonProperty("joined")] public DateTimeOffset Started { get; set; }
    }
}