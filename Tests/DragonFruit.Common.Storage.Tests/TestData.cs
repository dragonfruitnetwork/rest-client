// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Common.Storage.Tests
{
    public class TestData
    {
        /// <summary>
        ///     string value
        /// </summary>
        public string FirstName { get; set; } = "Bob";

        /// <summary>
        ///     string value
        /// </summary>
        public string LastName { get; set; } = "Dylan";

        /// <summary>
        ///     struct data for testing serialization of non-standard data types
        /// </summary>
        public DateTime DoB { get; set; } = DateTime.Parse("24-05-1941");

        /// <summary>
        ///     bool value
        /// </summary>
        public bool IsAlive { get; set; } = true;

        /// <summary>
        ///     small number for testing bytes
        /// </summary>
        public byte SmallNumber { get; set; } = 127;

        /// <summary>
        ///     number that can be converted to a short, int and long
        /// </summary>
        public short Year { get; set; } = 1941;

        /// <summary>
        ///     Test data for larger numbers. Cannot be converted to a short (but could become a ushort)
        /// </summary>
        public int LargeNumber { get; set; } = 33000;

        /// <summary>
        ///     Extreme number for parsing as int64
        /// </summary>
        public long VeryLargeNumber { get; set; } = 30000000000;

        /// <summary>
        ///     Nullable value for testing
        /// </summary>
        public int? NullValue { get; set; } = null;

        public double Pi { get; set; } = 3.14159;
    }
}