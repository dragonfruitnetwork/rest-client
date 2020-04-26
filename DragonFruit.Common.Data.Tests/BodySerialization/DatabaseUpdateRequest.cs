// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.Common.Data.Tests.BodySerialization
{
    internal class DatabaseUpdateRequest : ApiRequest
    {
        public override string Path => "https://postman-echo.com/post";

        public override Methods Method => Methods.Post;

        public override DataTypes DataType => DataTypes.SerializedProperty;

        [RequestBody]
        public Employee Employee { get; set; } = new Employee
        {
            Department = "R&D",
            Manager = "Alan",
            Name = "John",
            Started = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
    }
}
