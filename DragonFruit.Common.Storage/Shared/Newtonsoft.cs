// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Globalization;
using Newtonsoft.Json;

namespace DragonFruit.Common.Storage.Shared
{
    public static class Newtonsoft
    {
        /// <summary>
        ///     Changeable settings used by the other components in this library
        /// </summary>
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            Culture = new CultureInfo("en-US"),
            Formatting = Formatting.Indented
        };

        /// <summary>
        ///     <see cref="JsonSerializer" /> used by all common services
        /// </summary>
        public static JsonSerializer Serializer { get; private set; } = JsonSerializer.Create(SerializerSettings);

        /// <summary>
        ///     Updates the Serializer with the <see cref="SerializerSettings" />. Has immediate effect
        /// </summary>
        public static void UpdateSerializer()
        {
            Serializer = JsonSerializer.Create(SerializerSettings);
        }
    }
}