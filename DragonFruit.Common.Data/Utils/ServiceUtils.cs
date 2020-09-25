// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Utils
{
    public static class ServiceUtils
    {
        private static JsonSerializer _serialiser;

        public static JsonSerializer DefaultSerialiser
        {
            get => _serialiser ??= JsonSerializer.CreateDefault();
            set => _serialiser = value;
        }
    }
}
