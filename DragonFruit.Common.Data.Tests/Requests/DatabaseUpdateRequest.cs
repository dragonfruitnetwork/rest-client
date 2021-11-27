// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using DragonFruit.Common.Data.Parameters;
using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Tests.Requests.Objects
{
    internal class DatabaseUpdateRequest : ApiRequest
    {
        public override string Path => $"https://postman-echo.com/{Method.ToString().ToLower()}";

        public DatabaseUpdateRequest(Methods method)
        {
            Method = method;
        }

        protected override Methods Method { get; }

        protected override BodyType BodyType => BodyType.SerializedProperty;

        [RequestBody]
        public Employee Employee { get; set; } = new Employee
        {
            Department = "R&D",
            Manager = "Alan",
            Name = "John",
            Started = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
    }

    internal class Employee
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("manager")]
        public string Manager { get; set; }

        [JsonProperty("joined")]
        public DateTimeOffset Started { get; set; }
    }
}
