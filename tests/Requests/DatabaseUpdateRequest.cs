// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;
using DragonFruit.Data.Parameters;
using Newtonsoft.Json;

#endregion

namespace DragonFruit.Data.Tests.Requests
{
    internal class DatabaseUpdateRequest : ApiRequest
    {
        public DatabaseUpdateRequest(Methods method)
        {
            Method = method;
        }

        public override string Path => $"https://postman-echo.com/{Method.ToString().ToLower()}";

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
